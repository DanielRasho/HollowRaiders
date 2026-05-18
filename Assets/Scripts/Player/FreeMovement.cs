using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 25f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right;
    private Animator animator;

    private bool isDashing = false;
    public bool IsDashing => isDashing;
    private float dashCooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        Input_Manager.Instance.Actions.Player.Move.performed += OnMove;
        Input_Manager.Instance.Actions.Player.Move.canceled += OnMove;
        
        Input_Manager.Instance.Actions.Player.Sprint.performed += OnSprint;
        Input_Manager.Instance.Actions.Player.Sprint.canceled += OnSprint;
        
        Input_Manager.Instance.Actions.Player.Pause.performed += OnPause;
        Input_Manager.Instance.Actions.Player.Pause.canceled += OnPause;
        
        Input_Manager.Instance.Actions.Player.Interact.performed += OnInteract;
        Input_Manager.Instance.Actions.Player.Interact.canceled += OnInteract;
        
        Input_Manager.Instance.Actions.Player.Map.performed += OnShowMap;
        Input_Manager.Instance.Actions.Player.Map.canceled += OnShowMap;
    }

    private void OnDisable()
    {
        Input_Manager.Instance.Actions.Player.Move.performed -= OnMove;
        Input_Manager.Instance.Actions.Player.Move.canceled -= OnMove;
        
        Input_Manager.Instance.Actions.Player.Sprint.performed -= OnSprint;
        Input_Manager.Instance.Actions.Player.Sprint.canceled -= OnSprint;
        
        Input_Manager.Instance.Actions.Player.Pause.performed -= OnPause;
        Input_Manager.Instance.Actions.Player.Pause.canceled -= OnPause;
        
        Input_Manager.Instance.Actions.Player.Interact.performed -= OnInteract;
        Input_Manager.Instance.Actions.Player.Interact.canceled -= OnInteract;
        
        Input_Manager.Instance.Actions.Player.Map.performed -= OnShowMap;
        Input_Manager.Instance.Actions.Player.Map.canceled -= OnShowMap;
    }

    void Update()
    {
        if (!isDashing)
            rb.linearVelocity = moveInput * moveSpeed;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        // animator.SetBool("isWalking", true);

        if (ctx.canceled)
        {
          // animator.SetBool("isWalking", false);
        }

        moveInput = ctx.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput.normalized;

        // animator.SetFloat("InputX", moveInput.x);
        // animator.SetFloat("InputY", moveInput.y);
    }
    
    public void OnPause(InputAction.CallbackContext ctx)
    {
        Debug.Log("Player pressed pause");
    }

    public void OnShowMap(InputAction.CallbackContext ctx)
    {
        Debug.Log("Player pressed show map");
    }
    
    public void OnSprint (InputAction.CallbackContext ctx)
    {
        
        Debug.Log("Player pressed sprint");
    }
    
    public void OnInteract (InputAction.CallbackContext ctx)
    {
        Debug.Log("Player pressed Interact button");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Respawn"))
        {
            SceneGameManager.Instance
                .NewTransition()
                .Load(SceneDatabase.Scenes.MainMenu, SceneDatabase.Scenes.MainMenu, true)
                .Unload(SceneDatabase.Scenes.Lvl1)
                .Unload(SceneDatabase.Scenes.Lvl2)
                .Unload(SceneDatabase.Scenes.Lvl3)
                .WithOverlay()
                .Perform();
        }
    }
}
