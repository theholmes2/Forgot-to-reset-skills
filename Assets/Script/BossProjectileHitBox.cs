using UnityEngine;

public class BossProjectileHitBox : MonoBehaviour
{
    public float damage = 1f; // 임시 데미지
    public bool destroyOnHit = false; // 맞으면 사라질지
    public bool hitOnlyOnce = true; // 한 번만 맞출지

    private bool hasHit; // 이미 맞췄는지 확인
    private EnemyTraitController traitController; // 보스 데이터 참조

    private void Awake()
    {
        traitController = GetComponentInParent<EnemyTraitController>(); // 부모 보스 데이터 찾기
    }

    private void OnEnable()
    {
        hasHit = false; // 이펙트가 다시 켜질 때 타격 기록 초기화
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void TryDamagePlayer(GameObject target)
    {
        if (hitOnlyOnce && hasHit)
            return; // 이미 맞췄으면 중복 데미지 방지

        if (!target.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        float finalDamage = GetDamage();

        playerHealth.TakeDamage(finalDamage); // 플레이어에게 데미지

        hasHit = true;

        Debug.Log("보스 투사체 데미지: " + finalDamage);

        if (destroyOnHit)
            gameObject.SetActive(false); // 지금은 풀처럼 꺼두기
    }

    private float GetDamage()
    {
        //아래주석확인
        if (traitController == null)
            return damage;

        if (traitController.EnemyData == null)
            return damage;

        if (traitController.EnemyData.baseStats == null)
            return damage;

        return traitController.EnemyData.baseStats.attack; // EnemyData 공격력 사용
    }
}

// TODO:
// 보스 공격도 나중에는 SkillData 또는 BossSkillData로 분리한다.
// 현재는 EnemyData.baseStats.attack을 직접 사용하지만,
// 최종 구조에서는 공격마다 별도의 데이터가 필요하다.
//
// 예시:
// - Attack1: 기본 공격, damageMultiplier 1.0
// - Attack2: 강한 공격, damageMultiplier 3.0
// - FireBreath: 화염 속성, damageMultiplier 2.0, elementType Fire
// - Curse: 어둠 속성, damageMultiplier 0.5, 디버프 effects 포함
//
// 이렇게 분리하면:
// 1. 보스 패턴별 데미지 배율 관리 가능
// 2. 속성 상성 계산 가능
// 3. 격 차이 / 방어력 / 저항 계산 가능
// 4. 플레이어가 보스 스킬을 훔쳤을 때 같은 데이터를 재사용 가능
// 5. 밸런싱 데이터를 ScriptableObject 또는 엑셀 기반으로 관리 가능
//
// 최종 데미지 흐름:
// 사용자 기본 공격력
// × 스킬 damageMultiplier
// × 속성 배율
// × 격 보정
// × 방어/피해감소 보정
// = 최종 데미지

// TODO:
// 보스 공격도 나중에는 SkillData 계열 데이터로 분리한다.
// Attack1, Attack2, 브레스, 장판 같은 패턴마다
// damageMultiplier, elementType, effects를 따로 가진다.
// 이렇게 하면 보스 스킬을 플레이어가 훔쳐서 사용할 때도 같은 데이터를 재사용할 수 있다.