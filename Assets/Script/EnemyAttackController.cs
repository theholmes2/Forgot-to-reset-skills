using System.Collections;
using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    public Enemy enemy;              // 상태 제어 담당
    public EnemyMovement movement;   // 타겟/거리/공격 이동 담당

    public float attackCooldown = 2f;    // 공격 쿨타임
    

    private float lastAttackTime;
    private bool isAttacking;

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponent<Enemy>();

        if (movement == null)
            movement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        if (enemy == null)
            return;

        if (movement == null)
            return;

        if (enemy.currentState == Enemy.State.Dead)
            return;

        if (enemy.currentState == Enemy.State.Hit)
            return;

        if (isAttacking)
            return;

        // 추적 상태일 때만 공격 가능
        if (enemy.currentState != Enemy.State.Chase)
            return;

        // 공격 대상이 있어야 공격 가능
        if (!movement.HasTarget())
            return;

        // 추적 범위 밖이면 공격하지 않음
        if (!movement.IsTargetInChaseRange())
            return;

        // 공격 사거리 안에 들어와야 공격 가능
        if (!movement.IsTargetInAttackRange())
            return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        movement.StopMoving();

        enemy.ChangeState(Enemy.State.Attack);

        // 기본 적은 멈춤, 햄스터는 돌진
        yield return movement.AttackMoveRoutine();

        // 공격 도중 맞아서 Hit/Dead가 됐으면 공격 종료 처리만 하고 빠짐
        if (enemy.currentState != Enemy.State.Attack)
        {
            isAttacking = false;
            yield break;
        }


        if (enemy.currentState != Enemy.State.Dead)
            enemy.ChangeState(Enemy.State.Chase);

        isAttacking = false;
    }
}