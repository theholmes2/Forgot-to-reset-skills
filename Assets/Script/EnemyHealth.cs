using System; // 기존
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    public float currentHealth;

    public Enemy enemy;

    public event Action<EnemyHealth> OnDied; // 죽었을 때 알림

    private EnemyTraitController traitController; // EnemyData 연결 담당
    private bool isDead; // 풀 재사용 대비, 중복 사망 방지

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        traitController = GetComponent<EnemyTraitController>();

        ResetHealth(); //  Awake에서도 ResetHealth를 통해 체력 초기화
    }

    public void ResetHealth() //  풀에서 다시 꺼낼 때 체력 초기화
    {
        ApplyHealthFromEnemyData(); //  EnemyData 체력 반영

        currentHealth = maxHealth; // 현재 체력 초기화
        isDead = false; // 다시 살아있는 상태로 변경
    }

    private void ApplyHealthFromEnemyData() //  EnemyData의 체력 적용 전용 함수
    {
        if (traitController == null)
            traitController = GetComponent<EnemyTraitController>();

        if (traitController != null &&
            traitController.EnemyData != null &&
            traitController.EnemyData.baseStats != null)
        {
            maxHealth = traitController.EnemyData.baseStats.maxHp; // EnemyData의 최대 체력 적용
        }
    }

    public float GetHealthPercent() //  보스 체력 비율 기반 소환 패턴용
    {
        if (maxHealth <= 0f)
            return 0f;

        return currentHealth / maxHealth;
    }

    public void TakeDamage(float damage, Transform attacker)
    {
        if (!gameObject.activeInHierarchy)
            return; // 풀로 반환되어 비활성화된 적이면 데미지 무시

        if (enemy == null)
            return;

        if (isDead)
            return; //  이미 죽었으면 데미지 무시

        if (enemy.currentState == Enemy.State.Dead)
            return; // 죽은 적은 데미지를 받지 않음

        Debug.Log("Enemy 데미지 받음: " + damage);

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f; // 음수 체력 방지
            Die();
            return;
        }

        enemy.OnDamaged(attacker); // 누가 때렸는지 Enemy에게 전달
    }

    private void Die()
    {
        if (isDead)
            return; //  Die 중복 호출 방지

        isDead = true; // 죽은 상태 기록

        enemy.ChangeState(Enemy.State.Dead); // 먼저 죽음 상태로 전환

        OnDied?.Invoke(this); // 상태 변경 후 죽었다고 알림
    }
}