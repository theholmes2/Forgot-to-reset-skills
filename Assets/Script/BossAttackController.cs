using System.Collections;
using UnityEngine;

public class BossAttackController : MonoBehaviour
{
    public Enemy enemy; // 보스 상태 제어
    public Transform player; // 공격 대상

    public float attackRange = 2f; // 공격 가능 거리
    public float attackCooldown = 2f; // 공격 쿨타임
    public float attackMotionTime = 1f; // 공격 모션 유지 시간

    private float lastAttackTime; // 마지막 공격 시간
    private bool isAttacking; // 공격 중인지 확인

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponent<Enemy>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (enemy == null)
            return;

        if (player == null)
            return;

        if (enemy.currentState == Enemy.State.Dead)
            return;

        if (enemy.currentState == Enemy.State.Hit)
            return;

        if (isAttacking)
            return;

        float distance = Vector2.Distance(transform.position, player.position); // 플레이어와 거리 계산

        if (distance > attackRange)
            return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        enemy.ChangeState(Enemy.State.Attack); // 공격 상태로 변경
        
        yield return new WaitForSeconds(attackMotionTime); // 공격 모션 기다림

        if (enemy.currentState != Enemy.State.Dead)
            enemy.ChangeState(Enemy.State.Chase); // 공격 끝나면 다시 추적

        isAttacking = false;
    }
}