using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float idleTime = 3f;
    public float patrolTime = 3f;

    public float chaseRangeX = 8f;      // X축 기준 추적 유지 거리
    public float chaseRangeY = 5f;      // Y축 기준 추적 유지 거리
    public float attackRangeX = 1.5f;   // X축 기준 공격 거리
    public float attackRangeY = 1.5f;   // Y축 기준 공격 거리
    public float alertTime = 2f;        // 추적을 놓친 뒤 경계 시간

    public bool canJump;
    public float jumpPower = 6f;

    public Transform target;

    public Transform groundCheck;       // 발밑 바닥 확인
    public Transform frontGroundCheck;  // 앞쪽 절벽 확인
    public Transform wallCheck;         // 앞쪽 벽 확인

    public LayerMask groundLayer;
    public float groundCheckRadius = 0.15f;
    public float frontCheckRadius = 0.15f;
    public float wallCheckDistance = 0.4f;

    public Transform visualRoot; // 실제 그림만 뒤집을 대상
    public bool isMovingRight = true;      // 현재 이동 방향

    protected Enemy enemy;
    protected Rigidbody2D rb;

    protected float stateTimer;
    protected float alertTimer;

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        visualRoot = GetComponent<Transform>();
      
    }

    protected virtual void Start()
    {
        if (enemy != null)
            enemy.ChangeState(Enemy.State.Idle);

        stateTimer = idleTime;
    }

    protected virtual void Update()
    {
        if (enemy == null)
            return;

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
            stateTimer = patrolTime;
            enemy.ChangeState(Enemy.State.Patrol);
        }
    }

    protected virtual void UpdatePatrol()
    {
        stateTimer -= Time.deltaTime;

        MoveForward();

        if (NeedTurn())
            Flip();

        if (stateTimer <= 0f)
        {
            stateTimer = idleTime;
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

        // Alert 애니메이션은 Enemy에서 Idle처럼 처리 중
        // TODO: Alert 전용 애니메이션 추가하면 Enemy.EnterAlertState 수정

        if (alertTimer <= 0f)
        {
            target = null;
            stateTimer = idleTime;
            enemy.ChangeState(Enemy.State.Idle);
        }
    }

    protected void StartAlert()
    {
        alertTimer = alertTime;
        enemy.ChangeState(Enemy.State.Alert);
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
            return true;

        if (hasWall && !canJump)
            return true;

        if (hasWall && canJump)
            TryJump();

        return false;
    }

    protected virtual void TryJump()
    {
        if (!IsGrounded())
            return;

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
    }

    public void ClearTarget() // [추가] 풀 재사용 시 이전 타겟 제거
    {
        target = null;
    }
}