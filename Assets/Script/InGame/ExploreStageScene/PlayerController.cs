using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[InfoBox("탐험 스테이지에서 플레이어를 제어하는 컨트롤러", "탐험 스테이지에서 플레이어를 제어하는 컨트롤러로 \nExploreStageMaster의 currentInteract에 할당되어 사용됩니다.")]
public class ExplorePlayerController : MonoBehaviour
{
    [SerializeField] 
    private float moveSpeed = 5f;
    
    [SerializeField]
    private float jumpForce = 12f;

    [SerializeField]
    private Rigidbody2D rd;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private Vector2 moveInput;


    [SerializeField]
    [InfoBox("Input Action Asset에서 생성된 클래스 참조")]
    private PlayerInput playerInputs;

    [SerializeField]
    private ExploreStageMaster exploreStageMaster;

    void Reset()
    {
        exploreStageMaster = GameObject.Find("ExploreStageMaster").GetComponent<ExploreStageMaster>();
        
        rd = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

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
        rd = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // 이동 처리
        rd.linearVelocity = new Vector2(moveInput.x * moveSpeed, rd.linearVelocity.y);
        
        if(moveInput.x == 0 && isGrounded)
        {
            rd.linearVelocity = new Vector2(0, rd.linearVelocity.y);
            rd.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
        else
        {
            rd.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if(moveInput.x > 0)
        {
            spriteRenderer.flipX = false;   
        }
        else if(moveInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
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
        if(exploreStageMaster.currentInteract != null)
        {
            Interact();
        }
    }

    private void Jump()
    {
        rd.linearVelocity = new Vector2(rd.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

    private void Interact()
    {
        // 상호작용 로직
        exploreStageMaster.currentInteract.InteractEvent.Invoke();
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
