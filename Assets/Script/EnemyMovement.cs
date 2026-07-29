using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 3f;
    public float idleTime = 3f;
    public float patrolTime = 3f;

    [Header("Random Patrol")]
    public bool useRandomPatrol = true; // 몹마다 다른 움직임을 만들지
    public float randomIdleTimeMin = 1f; // 최소 대기 시간
    public float randomIdleTimeMax = 3f; // 최대 대기 시간
    public float randomPatrolTimeMin = 1f; // 최소 이동 시간
    public float randomPatrolTimeMax = 3f; // 최대 이동 시간
    public float randomFlipChance = 0.35f; // 순찰 시작 시 방향을 바꿀 확률
    public float randomStayChance = 0.25f; // 순찰 대신 다시 가만히 있을 확률

    [Header("Chase")]
    public float chaseRangeX = 8f;      // X축 기준 추적 유지 거리
    public float chaseRangeY = 5f;      // Y축 기준 추적 유지 거리
    public float attackRangeX = 1.5f;   // X축 기준 공격 거리
    public float attackRangeY = 1.5f;   // Y축 기준 공격 거리
    public float alertTime = 2f;        // 추적을 놓친 뒤 경계 시간

    [Header("Jump")]
    public bool canJump;
    public float jumpPower = 6f;
    protected bool canUseJump = true; // [추가] 착지 전까지 점프 중복 방지
    public float stepJumpChance = 0.8f; // 벽/계단 감지 시 실제 점프할 확률

    public Transform target;

    [Header("Check Points")]
    public Transform groundCheck;       // 발밑 바닥 확인
    public Transform frontGroundCheck;  // 앞쪽 절벽 확인
    public Transform wallCheck;         // 앞쪽 벽 확인

    public LayerMask groundLayer;
    public float groundCheckRadius = 0.15f;
    public float frontCheckRadius = 0.15f;
    public float wallCheckDistance = 0.4f;

    [Header("Step Check")]
    public float stepDownCheckDistance = 3f; // 이 거리 안에 아래 발판이 있으면 절벽이 아니라 계단/낮은 발판으로 판단
    public bool canStepDown = true; // 아래 발판으로 내려갈 수 있는지

    [Header("Visual")]
    public Transform visualRoot; // 실제 그림만 뒤집을 대상
    public bool isMovingRight = true; // 현재 이동 방향

    protected Enemy enemy;
    protected Rigidbody2D rb;

    protected float stateTimer;
    protected float alertTimer;
    

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();

        if (visualRoot == null)
            visualRoot = transform; // 비어 있으면 자기 자신 사용
    }

    protected virtual void Start()
    {
        if (enemy != null)
            enemy.ChangeState(Enemy.State.Idle);

        stateTimer = GetIdleDuration(); // 랜덤 대기 시간 적용
    }

    protected virtual void Update()
    {

        if (enemy == null)
            return;

        if (IsGrounded())
            canUseJump = true; // 바닥에 닿으면 다시 점프 가능

        if (!enemy.CanMove())
            return;

        if (enemy.currentState == Enemy.State.Idle)
        {
            UpdateIdle();
            return;
        }

        if (enemy.currentState == Enemy.State.Patrol)
        {
            UpdatePatrol();
            return;
        }

        if (enemy.currentState == Enemy.State.Chase)
        {
            UpdateChase();
            return;
        }

        if (enemy.currentState == Enemy.State.Alert)
        {
            UpdateAlert();
            return;
        }
    }

    public virtual void StartChase(Transform newTarget)
    {
        if (newTarget == null)
            return;

        target = newTarget;
        alertTimer = alertTime;

        if (enemy != null)
            enemy.ChangeState(Enemy.State.Chase);
    }

    protected virtual void UpdateIdle()
    {
        StopMove();

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            DecideNextPatrolAction(); // 바로 Patrol이 아니라 랜덤 행동 결정
        }
    }

    protected virtual void UpdatePatrol()
    {
        stateTimer -= Time.deltaTime;

        MoveForward();

        if (NeedTurn())
        {
            Flip();
            stateTimer = GetIdleDuration(); // 절벽/벽 만나면 잠깐 멈춤
            enemy.ChangeState(Enemy.State.Idle);
            return;
        }

        if (stateTimer <= 0f)
        {
            stateTimer = GetIdleDuration(); // 랜덤 대기 시간 적용
            enemy.ChangeState(Enemy.State.Idle);
        }
    }

    protected virtual void UpdateChase()
    {
        if (target == null)
        {
            StartAlert();
            return;
        }

        if (!IsTargetInChaseRange())
        {
            StartAlert();
            return;
        }

        // 공격 거리 안이면 더 붙지 않고 멈춤
        if (IsTargetInAttackRange())
        {
            StopMove();
            LookAtTarget();
            return;
        }

        MoveToTargetX();
    }

    protected virtual void UpdateAlert()
    {
        StopMove();

        // Alert 중에도 타겟이 다시 추적 범위 안으로 들어오면 추적 재개
        if (target != null && IsTargetInChaseRange())
        {
            enemy.ChangeState(Enemy.State.Chase);
            return;
        }

        alertTimer -= Time.deltaTime;

        // TODO: Alert 전용 애니메이션 추가하면 Enemy.EnterAlertState 수정

        if (alertTimer <= 0f)
        {
            target = null;
            stateTimer = GetIdleDuration(); // 랜덤 대기 시간 적용
            enemy.ChangeState(Enemy.State.Idle);
        }
    }

    protected void StartAlert()
    {
        alertTimer = alertTime;
        enemy.ChangeState(Enemy.State.Alert);
    }

    protected virtual void DecideNextPatrolAction() // 대기 후 다음 행동 랜덤 결정
    {
        if (useRandomPatrol)
        {
            if (Random.value < randomFlipChance)
                Flip(); // 랜덤하게 방향 전환

            if (Random.value < randomStayChance)
            {
                stateTimer = GetIdleDuration(); // 이동 안 하고 다시 대기
                enemy.ChangeState(Enemy.State.Idle);
                return;
            }
        }

        stateTimer = GetPatrolDuration();
        enemy.ChangeState(Enemy.State.Patrol);
    }

    protected float GetIdleDuration() // 랜덤 대기 시간
    {
        if (!useRandomPatrol)
            return idleTime;

        return Random.Range(randomIdleTimeMin, randomIdleTimeMax);
    }

    protected float GetPatrolDuration() // 랜덤 이동 시간
    {
        if (!useRandomPatrol)
            return patrolTime;

        return Random.Range(randomPatrolTimeMin, randomPatrolTimeMax);
    }

    protected void MoveForward()
    {
        float direction = isMovingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    protected void MoveToTargetX()
    {
        LookAtTarget();

        float direction = isMovingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (NeedTurn())
            StopMove();
    }

    protected void StopMove()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    protected void LookAtTarget()
    {
        if (target == null)
            return;

        bool targetIsRight = target.position.x > transform.position.x;

        if (targetIsRight != isMovingRight)
            Flip();
    }

    protected bool NeedTurn()
    {
        bool hasFrontGround = true;
        bool hasWall = false;

        if (frontGroundCheck != null)
            hasFrontGround = Physics2D.OverlapCircle(frontGroundCheck.position, frontCheckRadius, groundLayer);

        if (wallCheck != null)
        {
            Vector2 direction = isMovingRight ? Vector2.right : Vector2.left;
            hasWall = Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, groundLayer);
        }

        if (!hasFrontGround)
        {
            // 바로 앞에는 바닥이 없어도, 조금 아래에 발판이 있으면 내려갈 수 있음
            if (canStepDown && HasLowerGroundAhead())
                return false;

            return true; // 아래에도 발판이 없을 때만 진짜 절벽으로 보고 방향 전환
        }

        if (hasWall && canJump)
        {
            TryJump(); // 점프 가능한 몹이면 벽/계단 앞에서 점프 시도
            return false; // 점프 가능한 몹은 바로 방향 전환하지 않음
        }

        if (hasWall && !canJump)
            return true; // 점프 못 하면 방향 전환

        return false;
    }
    protected bool HasLowerGroundAhead()
    {
        if (frontGroundCheck == null)
            return false;

        // 앞쪽 검사 위치에서 아래로 긴 레이를 쏴서 낮은 발판이 있는지 확인
        RaycastHit2D hit = Physics2D.Raycast(
            frontGroundCheck.position,
            Vector2.down,
            stepDownCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }
    protected virtual void TryJump()
    {
        if (!canJump)
            return;

        if (!canUseJump)
            return; // 이미 점프했으면 착지 전까지 다시 점프 안 함

        if (!IsGrounded())
            return;

        if (Random.value > stepJumpChance)
            return; // 항상 점프하지 않고 확률로 점프

        canUseJump = false; // 점프 사용
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
    }

    protected bool IsGrounded()
    {
        if (groundCheck == null)
            return false;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    protected void Flip()
    {
        isMovingRight = !isMovingRight;
        ApplyLookDirection();
    }

    protected void ApplyLookDirection()
    {
        if (visualRoot == null)
            visualRoot = transform;

        Vector3 scale = visualRoot.localScale;
        scale.x = Mathf.Abs(scale.x) * (isMovingRight ? 1f : -1f);
        visualRoot.localScale = scale;
    }

    public Transform GetTarget()
    {
        return target;
    }

    public bool HasTarget()
    {
        return target != null;
    }

    public bool IsTargetInAttackRange()
    {
        if (target == null)
            return false;

        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D targetCollider = target.GetComponent<Collider2D>();

        // 콜라이더가 있으면 중심점이 아니라 콜라이더 표면 기준으로 거리 계산
        if (myCollider != null && targetCollider != null)
        {
            Vector2 myClosestPoint = myCollider.ClosestPoint(targetCollider.bounds.center);
            Vector2 targetClosestPoint = targetCollider.ClosestPoint(myCollider.bounds.center);

            float xDistance = Mathf.Abs(myClosestPoint.x - targetClosestPoint.x);
            float yDistance = Mathf.Abs(myClosestPoint.y - targetClosestPoint.y);

            return xDistance <= attackRangeX && yDistance <= attackRangeY;
        }

        // 콜라이더가 없으면 기존처럼 중심점 기준
        float fallbackXDistance = Mathf.Abs(target.position.x - transform.position.x);
        float fallbackYDistance = Mathf.Abs(target.position.y - transform.position.y);

        return fallbackXDistance <= attackRangeX && fallbackYDistance <= attackRangeY;
    }

    public bool IsTargetInChaseRange()
    {
        if (target == null)
            return false;

        float xDistance = Mathf.Abs(target.position.x - transform.position.x);
        float yDistance = Mathf.Abs(target.position.y - transform.position.y);

        return xDistance <= chaseRangeX && yDistance <= chaseRangeY;
    }

    public void StopMoving()
    {
        StopMove();
    }

    public virtual IEnumerator AttackMoveRoutine()
    {
        // 기본 적은 공격 중 별도 이동 없음
        yield break;
    }

    public void ClearTarget()
    {
        target = null; // 풀 재사용 시 이전 타겟 제거
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (frontGroundCheck != null)
            Gizmos.DrawWireSphere(frontGroundCheck.position, frontCheckRadius);

        if (wallCheck != null)
        {
            Vector3 direction = isMovingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawRay(wallCheck.position, direction * wallCheckDistance);
        }

        if (frontGroundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(frontGroundCheck.position, Vector3.down * stepDownCheckDistance);
        }
    }
}