using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hit,
        Dead,
    }
    public State currentState;

    public Transform player;
    public float speed = 3f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    public Animator anim;

    private bool isMovingRight = false;


    public Transform frontCheck; // 적 앞쪽 발끝 위치
    public LayerMask groundLayer; // 바닥으로 인식할 레이어
    public float rayLength = 1.0f; // 레이 발사 길이


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = State.Patrol;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    { 
        // 상태에 따른 로직 분기
        if (currentState == State.Dead)
            return;
        
      
        if (currentState == State.Patrol)
        {
            Patrol();
            anim.SetBool("1_Move",true);
        }
        else if (currentState == State.Chase)
        {
            Chase();
        }

        // 아래 방향으로 레이캐스트 발사
        bool isGrounded = Physics2D.Raycast(frontCheck.position, Vector2.down, rayLength, groundLayer);
       
      
        // 절벽 감지 시 방향 전환
        if (!isGrounded)
        {
            Flip();

        }




    }
    void Flip()
    {
        isMovingRight = !isMovingRight;// 방향 반전
        transform.Rotate(0, 180, 0);
        //spriteRenderer.flipX = rb.linearVelocityX>0? true: false;
               // 캐릭터 스프라이트 좌우 반전 로직 추가 가능

    }
    // Scene 뷰에서 레이캐스트 범위를 시각적으로 확인하기 위한 함수
    void OnDrawGizmos()
    {
        if (frontCheck != null)
        {
            Debug.DrawRay(frontCheck.position, Vector2.down * rayLength, Color.red);
        }
    }

    void Patrol()
    {
        // 좌우 이동 로직
        float direction = isMovingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    void Chase()
    {
        // 플레이어가 있는 방향으로 이동
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    public void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        if (currentState == State.Dead)
        {
            // 죽음 상태에 들어왔을 때
            EnterDeadState();
        }
    }

    private void EnterDeadState()
    {
        // 이동 멈추기
        rb.linearVelocity = Vector2.zero;
        // 콜라이더 끄기
      
        GetComponent<BoxCollider2D>().enabled = false;
        // 죽음 애니메이션 재생
        // 오브젝트 삭제
        Destroy(gameObject, 1f);
    }
}
