using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 25f;

    [Header("Dash")] 
    [SerializeField] private bool enableDash = true;
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.75f;

    [Header("Enemy Collision")]
    [Tooltip("Layer assigned to enemies.")]
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right;
    private Animator animator;
    private AfterImageTracer afterImageTracer;
    
    private bool canDash = true;
    private bool isDashing = false;
    private bool isInvulnerable = false;

    public bool IsDashing => isDashing;
    public bool IsInvulnerable => isInvulnerable;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        afterImageTracer = GetComponent<AfterImageTracer>();

        Input_Manager.Instance.Actions.Player.Move.performed += OnMove;
        Input_Manager.Instance.Actions.Player.Move.canceled += OnMove;

        Input_Manager.Instance.Actions.Player.Sprint.performed += OnDash;

        Input_Manager.Instance.Actions.Player.Interact.performed += OnInteract;
        Input_Manager.Instance.Actions.Player.Interact.canceled += OnInteract;
    }

    private void OnDisable()
    {
        Input_Manager.Instance.Actions.Player.Move.performed -= OnMove;
        Input_Manager.Instance.Actions.Player.Move.canceled -= OnMove;

        Input_Manager.Instance.Actions.Player.Sprint.performed -= OnDash;

        Input_Manager.Instance.Actions.Player.Interact.performed -= OnInteract;
        Input_Manager.Instance.Actions.Player.Interact.canceled -= OnInteract;
    }

    void Update()
    {
        if (!isDashing)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput.normalized;

        animator.SetBool("IsWalking", moveInput != Vector2.zero);
        animator.SetFloat("InputX", lastMoveDirection.x);
        animator.SetFloat("InputY", lastMoveDirection.y);
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !canDash || !enableDash)
            return;

        StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        isInvulnerable = true;

        // Ignore enemy collisions
        SetEnemyCollision(false);

        // Mouse position in world
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // Direction to mouse
        Vector2 dashDirection = (mouseWorldPos - transform.position).normalized;

        // Dash animation
        animator.SetTrigger("IsDashing");

        lastMoveDirection = dashDirection;

        animator.SetFloat("InputX", lastMoveDirection.x);
        animator.SetFloat("InputY", lastMoveDirection.y);

        // Apply dash velocity
        rb.linearVelocity = dashDirection * dashForce;

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            afterImageTracer.Emit();

            elapsed += Time.deltaTime;

            yield return null;
        }

        // Stop dash
        rb.linearVelocity = Vector2.zero;

        // Re-enable collisions
        SetEnemyCollision(true);

        isDashing = false;
        isInvulnerable = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void SetEnemyCollision(bool enabled)
    {
        int playerLayer = gameObject.layer;

        for (int i = 0; i < 32; i++)
        {
            if ((enemyLayer.value & (1 << i)) != 0)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, i, !enabled);
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Player pressed Interact button");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
    }
}