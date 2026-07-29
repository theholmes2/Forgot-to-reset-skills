using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Alert,
        Attack,
        Hit,
        Dead,
    }

    public State currentState;

    public float knockBack = 15f;

    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers;
    private Animator anim;
    private EnemyMovement movement;

    private Coroutine hitRoutine;
    private Transform lastAttacker; // 마지막으로 나를 때린 대상

    public float deadReturnTime = 0.66f; //  죽음 애니메이션 후 비활성화까지 걸리는 시간

    private EnemyHealth enemyHealth; // 풀 재사용 시 체력 초기화용
    private Collider2D enemyCollider; // 죽을 때 끄고, 재사용 때 다시 켤 콜라이더
    private int defaultLayer; //  DeadEnemy 레이어로 바꾼 뒤 원래 레이어 복구용
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        movement = GetComponent<EnemyMovement>();

        enemyHealth = GetComponent<EnemyHealth>(); 
        enemyCollider = GetComponent<BoxCollider2D>(); 
        defaultLayer = gameObject.layer; // 원래 레이어 저장

        currentState = State.Idle;
        EnterIdleState();
    }

    public void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        if (currentState == State.Idle)
        {
            EnterIdleState();
            return;
        }

        if (currentState == State.Patrol)
        {
            EnterPatrolState();
            return;
        }

        if (currentState == State.Chase)
        {
            EnterChaseState();
            return;
        }

        if (currentState == State.Alert)
        {
            EnterAlertState();
            return;
        }

        if (currentState == State.Attack)
        {
            EnterAttackState();
            return;
        }

        if (currentState == State.Hit)
        {
            EnterHitState();
            return;
        }

        if (currentState == State.Dead)
        {
            EnterDeadState();
            return;
        }
    }

    public bool CanMove()
    {
        // 이동 가능한 상태만 Movement가 처리
        return currentState == State.Idle ||
               currentState == State.Patrol ||
               currentState == State.Chase ||
               currentState == State.Alert;
    }

    public void OnDamaged(Transform attacker)
    {
        if (!gameObject.activeInHierarchy)
            return; // 비활성 상태면 피격 처리 안 함

        if (currentState == State.Dead)
            return;
        lastAttacker = attacker; // 넉백 방향 계산용
        // 누가 때렸는지 Movement에게 넘겨서 그 대상을 추적
        if (movement != null)
            movement.StartChase(attacker);

        ChangeState(State.Hit);
    }

    private void EnterIdleState()
    {
        // 대기 상태 애니메이션
        SetMoveAnimation(false);
    }

    private void EnterPatrolState()
    {
        // 순찰 상태 애니메이션
        SetMoveAnimation(true);
    }

    private void EnterChaseState()
    {
        // 추적 상태 애니메이션
        SetMoveAnimation(true);
    }

    private void EnterAlertState()
    {
        // 경계 상태
        // TODO: Alert 전용 애니메이션 추가하면 여기에서 교체
        // 지금은 Alert 애니메이션이 없으니 Idle처럼 보이게 처리

        SetMoveAnimation(false);
    }

    private void EnterAttackState()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        SetMoveAnimation(true);

        if (anim != null)
        {
            anim.ResetTrigger("Attack1");
            anim.SetTrigger("Attack1");
        }
    }

    private void EnterHitState()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        SetMoveAnimation(true);

        if (anim != null)
            anim.SetTrigger("isHurt");

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {

        ApplyKnockBack(); // 피격 즉시 밀림

       

        yield return new WaitForSeconds(0.25f);

       

        if (currentState != State.Dead)
            ChangeState(State.Chase);
    }

    private void ApplyKnockBack()
    {
        if (rb == null)
            return;

        if (lastAttacker == null)
            return;

        // 공격자 반대 방향으로 밀림
        Vector2 knockBackDirection = transform.position - lastAttacker.position;
        knockBackDirection.y = 0.3f; // 살짝 위로 뜨게 보정
        knockBackDirection.Normalize();

        rb.AddForce(knockBackDirection * knockBack, ForceMode2D.Impulse);
    }

    private void EnterDeadState()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

        if (anim != null)
            anim.SetTrigger("isDie");

        SetSpriteColor(Color.gray);

       
        StartCoroutine(DeadRoutine()); //  죽음 애니 후 비활성화
    }
    private IEnumerator DeadRoutine() // 죽음 애니메이션 후 풀 반환 전 단계
    {
        yield return new WaitForSeconds(deadReturnTime);

        PooledEnemy pooledEnemy = GetComponent<PooledEnemy>();

        if (pooledEnemy != null)
        {
            pooledEnemy.ReturnToPool(); // 풀로 반환
            yield break;
        }

        gameObject.SetActive(false); 
    }

    public void ResetEnemy() //  풀에서 다시 꺼낼 때 호출할 초기화 함수
    {
        StopAllCoroutines(); // 이전 피격/죽음 루틴 정리

        currentState = State.Idle; // 기본 상태로 복구
        lastAttacker = null; // 이전 공격자 정보 제거

        if (movement != null)
            movement.ClearTarget(); //   이전 추적 대상 제거

        gameObject.layer = defaultLayer; // DeadEnemy 레이어에서 원래 레이어로 복구

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (enemyCollider != null)
            enemyCollider.enabled = true; // 콜라이더 다시 켜기

        if (enemyHealth != null)
            enemyHealth.ResetHealth(); // EnemyData 반영 포함 체력 초기화

        SetSpriteColor(Color.white); // 죽을 때 회색 처리한 것 복구

        if (anim != null)
        {
            anim.ResetTrigger("Attack1");
            anim.ResetTrigger("isHurt");
            anim.ResetTrigger("isDie");

            anim.SetBool("1_Move", false);

            anim.SetTrigger("Respawn"); // Die -> Idle 연결용 트리거
        }

       
    }
    private void SetMoveAnimation(bool isMove)
    {
        if (anim == null)
            return;

        anim.SetBool("1_Move", isMove);
        
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

    public void ForceDead() // [추가] 보스 사망/구역 이탈 등으로 강제로 죽일 때 사용
    {
        if (currentState == State.Dead)
            return;

        ChangeState(State.Dead); // 죽음 애니메이션 후 DeadRoutine에서 풀 반환
    }

}