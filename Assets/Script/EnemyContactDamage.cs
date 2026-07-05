using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{

    public float damageInterval = 1f; // 데미지 간격

    private float lastDamageTime; // 마지막 데미지 시간
    private Enemy enemy; // 적 상태 확인용

    private EnemyTraitController traitController;

    private void Awake()
    {
        enemy = GetComponent<Enemy>(); // 같은 오브젝트의 Enemy 가져오기
        traitController = GetComponent<EnemyTraitController>();
    }

    private float GetAttackDamage()
    {
        if (traitController == null)
            return 1f;

        if (traitController.EnemyData == null)
            return 1f;

        if (traitController.EnemyData.baseStats == null)
            return 1f;

        return traitController.EnemyData.baseStats.attack;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject); // 충돌 중인 대상 검사
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDamagePlayer(collision.gameObject); // 트리거 중인 대상 검사
    }

    private void TryDamagePlayer(GameObject target)
    {
        float damage = GetAttackDamage();

        if (enemy != null && enemy.currentState == Enemy.State.Dead)
            return; // 죽은 적은 데미지 안 줌

        if (!target.CompareTag("Player"))
            return; // 플레이어만 데미지

        if (Time.time < lastDamageTime + damageInterval)
            return; // 아직 데미지 쿨타임이면 종료

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;
        

        playerHealth.TakeDamage(damage); // 플레이어에게 데미지
        lastDamageTime = Time.time; // 데미지 시간 저장

        Debug.Log("플레이어 접촉 데미지: " + damage);
    }
}