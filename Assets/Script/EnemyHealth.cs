using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    public float currentHealth;

    public Enemy enemy;

    private EnemyTraitController traitController; // EnemyData 연결 담당

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        traitController = GetComponent<EnemyTraitController>();

        if (traitController != null &&
            traitController.EnemyData != null &&
            traitController.EnemyData.baseStats != null)
        {
            // EnemyData의 최대 체력을 실제 적에게 적용
            maxHealth = traitController.EnemyData.baseStats.maxHp;
        }

        currentHealth = maxHealth; // 현재 체력 초기화
    }

    public void TakeDamage(float damage)
    {
        if (enemy == null)
            return;

        if (enemy.currentState == Enemy.State.Dead)
            return; // 죽은 적은 데미지를 받지 않음

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f; // 음수 체력 방지
            Die();
            return;
        }

        enemy.ChangeState(Enemy.State.Hit); // 살아있으면 피격 상태
    }

    private void Die()
    {
        enemy.ChangeState(Enemy.State.Dead);
    }
}