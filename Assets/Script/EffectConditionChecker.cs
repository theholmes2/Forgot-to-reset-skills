using UnityEngine;

public class EffectConditionChecker : MonoBehaviour
{
    public bool CanApplyEffect(EffectData effectData, PlayerStatController user, EnemyTraitController target)
    {
        // TODO:
        // 1. requiredTargetTags 조건 만족하는지
        // 2. forbiddenTargetTags에 걸리는지
        // 3. requiredUserElements 조건 만족하는지
        // 4. forbiddenUserElements에 걸리는지
        // 5. 보스/비행 대상 허용 여부 확인
        return false;
    }

    public bool CheckTargetTags(EffectData effectData, EnemyTraitController target)
    {
        // TODO:
        // 1. requiredTargetTags / forbiddenTargetTags 검사
        return false;
    }

    public bool CheckUserElements(EffectData effectData, PlayerStatController user)
    {
        // TODO:
        // 1. requiredUserElements / forbiddenUserElements 검사
        return false;
    }

    public bool CheckBossRestriction(EffectData effectData, EnemyTraitController target)
    {
        // TODO:
        // 1. target이 Boss 태그인데 canAffectBoss가 false면 막기
        return false;
    }

    public bool CheckFlightRestriction(EffectData effectData, EnemyTraitController target)
    {
        // TODO:
        // 1. target이 Flying 태그인데 canAffectFlying이 false면 막기
        return false;
    }
}
