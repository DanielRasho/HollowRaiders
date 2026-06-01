using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [Header("GUI")]
    [SerializeField] private GameObject gui;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image leftPosition;
    [SerializeField] private Image rightPosition;

    [Header("Atlas")]
    [SerializeField] private List<CharacterDefinition> charactersDic;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private int charactersBetweenSound = 1;

    private bool isDialogueRunning;
    private bool isPrintingText;

    private DialogActivator currentDialogue;
    private int currentLineIndex;

    private Coroutine typingCoroutine;

    private readonly Dictionary<CharacterID, CharacterDefinition> characters =
        new Dictionary<CharacterID, CharacterDefinition>();

    private void Awake()
    {
        foreach (var character in charactersDic)
        {
            if (!characters.ContainsKey(character.id))
                characters.Add(character.id, character);
        }
    }

    private void Start()
    {
        DialogActivator.OnStartDialogue += StartDialogue;
        Input_Manager.Instance.Actions.Dialogue.Next.performed += NextLine;
    }

    private void OnDisable()
    {
        DialogActivator.OnStartDialogue -= StartDialogue;
        Input_Manager.Instance.Actions.Dialogue.Next.performed -= NextLine;
    }

    private void StartDialogue(DialogActivator activator)
    {
        if (isDialogueRunning)
            return;

        currentDialogue = activator;
        currentLineIndex = 0;

        Input_Manager.Instance.SwitchToMap(InputMap.DIALOGUE);

        isDialogueRunning = true;

        ShowDialogueUI(true);

        ShowLine(currentLineIndex);
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        currentDialogue = null;
        currentLineIndex = 0;

        isDialogueRunning = false;
        isPrintingText = false;

        characterName.text = "";
        text.text = "";

        leftPosition.sprite = null;
        rightPosition.sprite = null;

        ShowDialogueUI(false);

        Input_Manager.Instance.SwitchToMap(InputMap.PLAYER);
    }

    private void ShowLine(int index)
    {
        DialogLine line = currentDialogue.lines[index];

        if (!characters.TryGetValue(line.characterToShow, out CharacterDefinition character))
        {
            Debug.LogError($"Character {line.characterToShow} not found.");
            return;
        }

        characterName.text = character.displayName;

        Sprite portrait = character.GetPortrait(line.characterExpression);

        if (line.characterPosition == CharacterPosition.Left)
        {
            leftPosition.sprite = portrait;
            leftPosition.color = new Color(1, 1, 1, 1);
            rightPosition.color = new Color(0, 0, 0, 0);
        }
        else
        {
            rightPosition.sprite = portrait;
            rightPosition.color = new Color(1, 1, 1, 1);
            leftPosition.color = new Color(0, 0, 0, 0);
        }

        text.text = line.LineText;

        // Important for TMP
        text.ForceMeshUpdate();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(AnimateText(line, character));
    }

    private IEnumerator AnimateText(DialogLine line, CharacterDefinition character)
    {
        isPrintingText = true;

        text.maxVisibleCharacters = 0;

        int totalCharacters = text.textInfo.characterCount;

        for (int i = 0; i <= totalCharacters; i++)
        {
            text.maxVisibleCharacters = i;

            if (i < totalCharacters)
            {
                char currentCharacter = line.LineText[i];

                if (i % charactersBetweenSound == 0)
                    AudioManager.Instance.PlayVoice(character.voice, character.voicePitch);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        text.maxVisibleCharacters = totalCharacters;

        isPrintingText = false;
    }

    private void ShowDialogueUI(bool status)
    {
        gui.SetActive(status);
    }

    private void NextLine(InputAction.CallbackContext ctx)
    {
        if (!isDialogueRunning)
            return;

        // Skip typing animation
        if (isPrintingText)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            text.maxVisibleCharacters = text.textInfo.characterCount;
            isPrintingText = false;
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.lines.Count)
        {
            EndDialogue();
            return;
        }

        ShowLine(currentLineIndex);
    }
}