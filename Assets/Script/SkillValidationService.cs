using UnityEngine;

public class SkillValidationService : MonoBehaviour
{
    public PlayerStatController playerStatController;
    public EquipmentController equipmentController;
    public ElementResolver elementResolver;
    public RankResolver rankResolver;

    public RestrictionResult CanUseSkill(SkillData skillData, EnemyTraitController target)
    {
        // TODO:
        // 1. 격 요구치 확인
        // 2. 플레이어 속성과 스킬 속성 충돌 확인
        // 3. 장비 속성과 스킬 속성 충돌 확인
        // 4. 패시브와 충돌 확인
        // 5. target 조건 확인
        // 6. 최종 RestrictionResult 반환
        return new RestrictionResult();
    }

    public RestrictionResult CheckRankRequirement(SkillData skillData)
    {
        // TODO:
        // 1. player rank와 skill requiredRank 비교
        // 2. 부족하면 Block 또는 Penalty 결과 반환
        return new RestrictionResult();
    }

    public RestrictionResult CheckElementRestriction(SkillData skillData)
    {
        // TODO:
        // 1. 플레이어 주 속성과 스킬 속성 충돌 여부 확인
        // 2. 예: Light면 Dark 스킬 사용 불가
        return new RestrictionResult();
    }

    public RestrictionResult CheckEquipmentRestriction(SkillData skillData)
    {
        // TODO:
        // 1. 현재 장비 속성과 스킬 속성 충돌 여부 확인
        // 2. 예: 물 장비 착용 중 불 스킬 패널티
        return new RestrictionResult();
    }

    public RestrictionResult CheckTargetRestriction(SkillData skillData, EnemyTraitController target)
    {
        // TODO:
        // 1. 특정 적 상대로만 사용 가능한 스킬인지 확인
        // 2. 사용 불가 / 패널티 여부 반환
        return new RestrictionResult();
    }
}
