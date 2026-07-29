using System.Collections;
using UnityEngine;

public class HamsterMovement : EnemyMovement
{
    [Header("Hamster Dash Attack")]
    public float dashSpeed = 8f;
    public float dashTime = 0.25f;
    public float dashReadyTime = 0.15f;

    [Header("Random Dash")]
    public bool useRandomDash = true; //  공격 돌진을 랜덤하게 할지
    public float randomDashSpeedMin = 6f; //  최소 돌진 속도
    public float randomDashSpeedMax = 10f; // 최대 돌진 속도
    public float randomDashTimeMin = 0.15f; //  최소 돌진 시간
    public float randomDashTimeMax = 0.35f; //  최대 돌진 시간
    public float dashJumpChance = 0.25f; //  돌진 시작할 때 살짝 점프할 확률
    public float dashJumpPower = 3f; // 돌진 점프 힘

    protected override void Start()
    {
        base.Start();

        canJump = true; //  햄스터는 계단/낮은 벽에서 점프 가능하게 기본 설정
    }

    protected override void UpdateChase()
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

        // 햄스터도 공격 거리 안에서는 더 붙지 않고 멈춤
        if (IsTargetInAttackRange())
        {
            StopMove();
            LookAtTarget();
            return;
        }

        MoveToTargetX();
    }

    protected override void TryJump()
    {
        // 햄스터는 기본 점프 로직 그대로 사용
        // TODO: 나중에 햄스터 전용 점프 애니메이션이 생기면 여기서 연결
        base.TryJump();
    }

    public override IEnumerator AttackMoveRoutine()
    {
        // 햄스터는 공격할 때 잠깐 멈춘 뒤 앞으로 돌진
        StopMove();
        LookAtTarget();

        yield return new WaitForSeconds(dashReadyTime);

        // 준비 중 맞았으면 돌진하지 않음
        if (enemy.currentState != Enemy.State.Attack)
            yield break;

        float currentDashSpeed = dashSpeed; //  이번 공격에 쓸 돌진 속도
        float currentDashTime = dashTime; //  이번 공격에 쓸 돌진 시간

        if (useRandomDash)
        {
            currentDashSpeed = Random.Range(randomDashSpeedMin, randomDashSpeedMax); //  랜덤 돌진 속도
            currentDashTime = Random.Range(randomDashTimeMin, randomDashTimeMax); //  랜덤 돌진 거리 느낌
        }

        float direction = isMovingRight ? 1f : -1f;

        if (canJump && IsGrounded() && Random.value < dashJumpChance)
        {
            rb.linearVelocity = new Vector2(direction * currentDashSpeed, dashJumpPower); //  가끔 뛰면서 돌진
        }
        else
        {
            rb.linearVelocity = new Vector2(direction * currentDashSpeed, rb.linearVelocity.y);
        }

        yield return new WaitForSeconds(currentDashTime);

        // 돌진 중 맞었으면 Hit 쪽 넉백을 유지해야 하므로 여기서 덮지 않음
        if (enemy.currentState != Enemy.State.Attack)
            yield break;

        StopMove();
    }
}