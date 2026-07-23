using System.Collections;
using UnityEngine;

public class HamsterMovement : EnemyMovement
{
    public float dashSpeed = 8f;
    public float dashTime = 0.25f;
    public float dashReadyTime = 0.15f;

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

    public override IEnumerator AttackMoveRoutine()
    {
        // 햄스터는 공격할 때 잠깐 멈춘 뒤 앞으로 돌진
        StopMove();
        LookAtTarget();

        yield return new WaitForSeconds(dashReadyTime);

        // 준비 중 맞았으면 돌진하지 않음
        if (enemy.currentState != Enemy.State.Attack)
            yield break;

        float direction = isMovingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(direction * dashSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(dashTime);

        // 돌진 중 맞었으면 Hit 쪽 넉백을 유지해야 하므로 여기서 덮지 않음
        if (enemy.currentState != Enemy.State.Attack)
            yield break;

        StopMove();
    }
}