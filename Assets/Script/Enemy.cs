using System.Collections;
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
    public float knockBack = 15f;

    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers; // 자식 포함 모든 스프라이트
    public Animator anim;

    private bool isMovingRight = false;

    public Transform floorCheck; // 적 앞쪽 발끝 위치
    public LayerMask groundLayer; // 바닥으로 인식할 레이어
    public float rayLength = 1.0f; // 레이 발사 길이

    private EnemyTraitController traitController; // EnemyData 연결 담당

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        traitController = GetComponent<EnemyTraitController>();

        currentState = State.Patrol;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (traitController != null &&
            traitController.EnemyData != null &&
            traitController.EnemyData.baseStats != null)
        {
            speed = traitController.EnemyData.baseStats.moveSpeed;
            knockBack = traitController.EnemyData.baseStats.knockBack;
        }
    }

    void Update()
    {
        if (currentState == State.Dead)
            return;

        if (currentState == State.Hit)
            return;

        if (currentState == State.Attack)
            return; // 공격 중에는 이동/절벽체크 안 함

        if (currentState == State.Patrol)
        {
            Patrol();

            if (anim != null)
                anim.SetBool("1_Move", true);
        }
        else if (currentState == State.Chase)
        {
            Chase();

            if (anim != null)
                anim.SetBool("1_Move", true);
        }

        if (floorCheck == null)
            return;

        bool isGrounded = Physics2D.Raycast(floorCheck.position, Vector2.down, rayLength, groundLayer);

        if (!isGrounded)
        {
            Flip();
        }
    }

    void Flip()
    {
        isMovingRight = !isMovingRight;
        transform.Rotate(0, 180, 0);
    }

    void OnDrawGizmos()
    {
        if (floorCheck != null)
        {
            Debug.DrawRay(floorCheck.position, Vector2.down * rayLength, Color.red);
        }
    }

    void Patrol()
    {
        float direction = isMovingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    void Chase()
    {
        if (player == null)
            return;

        float direction = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        if (player.position.x < transform.position.x)
        {
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
            EnterDeadState();
            return;
        }

        if (currentState == State.Hit)
        {
            EnterHitState();
            return;
        }

        if (currentState == State.Attack)
        {
            EnterAttackState();
            return;
        }
    }

    private void EnterAttackState()
    {
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("1_Move", false);
            anim.ResetTrigger("Attack1");
            anim.SetTrigger("Attack1");
        }
    }

    private void EnterHitState()
    {
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("1_Move", false);
             anim.SetTrigger("isHurt"); 
        }

        StartCoroutine(HitKnockBack());
    }

    IEnumerator HitKnockBack()
    {
        if (player != null)
        {
            rb.AddForce((rb.transform.position - player.transform.position).normalized * 1.5f * knockBack, ForceMode2D.Impulse);
        }

        rb.AddForce(Vector2.up * knockBack, ForceMode2D.Impulse);

        SetSpriteColor(Color.red);

        yield return new WaitForSeconds(0.3f);

        SetSpriteColor(Color.white);

        ChangeState(State.Chase);
    }

    private void EnterDeadState()
    {
        rb.linearVelocity = Vector2.zero;

        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

        if (anim != null)
        {
            anim.SetBool("1_Move", false);
            anim.SetTrigger("isDie"); 
        }

        SetSpriteColor(Color.gray);

        StartCoroutine(DeadKnockBack());

        Destroy(gameObject, 1f);
    }

    IEnumerator DeadKnockBack()
    {
        if (player != null)
        {
            rb.AddForce((rb.transform.position - player.transform.position).normalized * knockBack, ForceMode2D.Impulse);
        }

        rb.AddForce(Vector2.up * 2 * knockBack, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.7f);

        rb.AddForce(Vector2.up * knockBack, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Attack || currentState == State.Hit || currentState == State.Dead)
            return;

        if (collision.transform.position.x - transform.position.x > 0.5f && isMovingRight)
        {
            Flip();
        }
        else if (collision.transform.position.x - transform.position.x < -0.5f && !isMovingRight)
        {
            Flip();
        }
    }

    private void SetSpriteColor(Color color)
    {
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer == null)
                continue;

            renderer.color = color;
        }
    }
}