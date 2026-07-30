using UnityEngine;

public class CombatResolver : MonoBehaviour
{
    public PlayerStatController playerStatController;
    public EffectConditionChecker effectConditionChecker;
    public RankResolver rankResolver;
    public ElementResolver elementResolver;

    private void Awake()
    {
        if (playerStatController == null)
            playerStatController = GetComponent<PlayerStatController>();
    }
    public float GetFinalAttackDamage(SkillData skillData)
    {

        if (skillData == null) // 스킬 검사
            return 0f;

        if (playerStatController == null) // 스탯 관리자 검사
            return 0f;

        // 버프가 적용된 최종 공격력
        float attackPower = playerStatController.GetFinalStat(StatType.Attack);

        // 최종 공격력에 스킬 데미지 배율 적용
        float finalDamage = attackPower * skillData.damageMultiplier;

        // 나중에 속성, 치명타, 적 방어력 등을 여기서 계산
        return finalDamage;
    }
    public void ResolveAttack(EnemyTraitController target)
    {
        // TODO:
        // 1. 기본 공격 컨텍스트 생성
        // 2. 최종 공격력 계산
        // 3. OnAttack / OnHit 효과 처리
        // 4. 최종 피해 적용
    }

    public void ResolveSkill(SkillData skillData, EnemyTraitController target)
    {
        // TODO:
        // 1. 스킬 기본 수치 계산
        // 2. skillData.effects 순회
        // 3. EffectConditionChecker 검사
        // 4. ElementResolver 계산
        // 5. RankResolver 계산
        // 6. 최종 데미지/버프/디버프 적용
    }

    public float ResolveDamage(float baseDamage, ElementType skillElement, EffectData effectData, EnemyTraitController target)
    {
        // TODO:
        // 1. 기본 데미지 시작
        // 2. 속성 배율 적용
        // 3. 격 패널티 적용
        // 4. 최종 피해 반환
        return baseDamage;
    }

    public void ApplyTriggeredEffects(TriggerType triggerType, EnemyTraitController target)
    {
        // TODO:
        // 1. 현재 활성 효과/장비 효과 중 해당 triggerType인 것 찾기
        // 2. 조건 맞는 효과만 적용
    }

    public void ApplyDamageToTarget(EnemyTraitController target, float finalDamage)
    {
        // TODO:
        // 1. target의 체력 시스템에 finalDamage 전달
        // 2. 적 사망 체크
    }


}
