using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

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
    public float knockBack = 40f;
    

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    public Animator anim;

    private bool isMovingRight = false;


    public Transform floorCheck; // 적 앞쪽 발끝 위치
    public LayerMask groundLayer; // 바닥으로 인식할 레이어
    public float rayLength = 1.0f; // 레이 발사 길이


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = State.Patrol;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
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
        bool isGrounded = Physics2D.Raycast(floorCheck.position, Vector2.down, rayLength, groundLayer);
       
      
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
        
               // 캐릭터 스프라이트 좌우 반전 로직 추가 가능

    }
    // Scene 뷰에서 레이캐스트 범위를 시각적으로 확인하기 위한 함수
    void OnDrawGizmos()
    {
        if (floorCheck != null)
        {
            Debug.DrawRay(floorCheck.position, Vector2.down * rayLength, Color.red);
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


        //플레이어 쳐다보기
        if (player.position.x < transform.position.x) 
        {  // 플레이어의 위치가 적의 위치보다 왼쪽에 있으면 0도
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }


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
            return;
        }
        
        if (currentState == State.Hit)
        {
            EnterHitState();
            return;
        } 


    }
    private void EnterHitState()
    {
        rb.linearVelocity = Vector2.zero; //움직임 멈추고
       
        StartCoroutine(HitKnockBack());//색 바뀌고
        
    }
    IEnumerator HitKnockBack()
    {
        rb.AddForce((rb.transform.position - player.transform.position).normalized * 1.5f* knockBack, ForceMode2D.Impulse); //민 다음
        rb.AddForce(Vector2.up * 1 * knockBack, ForceMode2D.Impulse); //작게뛰고
        spriteRenderer.color = Color.red; //빨개짐
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = Color.white;
        ChangeState(State.Chase); //플레이어한테 감
    }

    private void EnterDeadState()
    {
        // 이동 멈추기
        rb.linearVelocity = Vector2.zero;
        //레이어 변경
        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

        // 죽음 애니메이션 재생
        spriteRenderer.color = Color.gray; //회색됨
        
        StartCoroutine(DeadKnockBack()); //죽으면 통통 튐

        // 오브젝트 삭제
        Destroy(gameObject, 1f);
    }

    IEnumerator DeadKnockBack()
    {
        rb.AddForce((rb.transform.position - player.transform.position).normalized * knockBack, ForceMode2D.Impulse); //뒤로밀고
        rb.AddForce(Vector2.up * 2 * knockBack, ForceMode2D.Impulse);//크게 뛰고
        yield return new WaitForSeconds(0.7f);
        rb.AddForce(Vector2.up *1* knockBack, ForceMode2D.Impulse); //작게뛰고
    }

    private void OnCollisionEnter2D(Collision2D collision)
    { 
        
      if (collision.transform.position.x -transform.position.x  > 0.5f &&isMovingRight)
        {
            Flip();
        }
      else if (collision.transform.position.x - transform.position.x < -0.5f && !isMovingRight)
        {
            Flip();
        }

    }


}
