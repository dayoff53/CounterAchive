using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    private float moveSpeed = 5f;
    
    [SerializeField]
    private float jumpForce = 12f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector2 moveInput;

    // Input Action Asset에서 생성된 클래스 참조
    private PlayerInput playerInputs;

    void Awake()
    {
        // Input Actions 초기화
        playerInputs = new PlayerInput();


        // 이벤트 바인딩
        playerInputs.Player.Move.performed += OnMove;
        playerInputs.Player.Move.canceled += OnMove;
        playerInputs.Player.Jump.performed += OnJump;
        playerInputs.Player.Interact.performed += OnInteract;

    }

    void OnEnable()
    {
        playerInputs.Enable();
    }

    void OnDisable()
    {
        playerInputs.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // 이동 처리
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            Jump();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Interact();
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

    private void Interact()
    {
        // 상호작용 로직 구현
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // 바닥에 닿았는지 체크
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 바닥에서 떨어졌는지 체크
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
