using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    private float moveSpeed = 5f;
    
    [SerializeField]
    private float jumpForce = 12f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;

    float height = 0;
    float width = 0;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();        
        
        height = Camera.main.orthographicSize;
        width = height * Screen.width / Screen.height;
    }

    void Update()
    {
        // 좌우 이동 입력 처리
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // 점프 입력 처리 (W키 또는 위쪽 방향키)
        if((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded)
        {
            Jump();
        }

        // 상호작용 입력 처리 (S키 또는 아래쪽 방향키)
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Interact();
        }
    }

    void FixedUpdate()
    {
        // 이동 처리
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
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
