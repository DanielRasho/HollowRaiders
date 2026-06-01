using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterID
{
    Belle,
    Seller,
    Anby,
    Boss,
}
public enum CharacterExpression
{
    Neutral,
    Happy,
    Sad,
    Angry,
    Think
}

public enum CharacterPosition {
    Right,
    Left
}

[Serializable]
public class EmotionData
{
    public CharacterExpression emotion;
    public Sprite portrait;
}

[CreateAssetMenu(
    fileName = "Character",
    menuName = "Dialogue/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    [Header("Character")]
    public CharacterID id;
    public string displayName;
    public AudioClip voice;
    public float voicePitch = 1f;

    [Header("Available Emotions")]
    public List<EmotionData> emotions = new();
    public Sprite defaultPortrait;

    public Sprite GetPortrait(CharacterExpression emotion)
    {
        foreach (var emotionData in emotions)
        {
            if (emotionData.emotion == emotion)
                return emotionData.portrait;
        }

        return defaultPortrait;
    }

    public bool HasEmotion(CharacterExpression emotion)
    {
        foreach (var emotionData in emotions)
        {
            if (emotionData.emotion == emotion)
                return true;
        }

        return false;
    }
}
