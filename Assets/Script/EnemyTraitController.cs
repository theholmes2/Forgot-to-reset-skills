using UnityEngine;

public class EnemyTraitController : MonoBehaviour
{
    public EnemyData enemyData; // 적 데이터 원본

    public EnemyData EnemyData => enemyData; // 다른 컴포넌트에 데이터 제공

    public bool HasTag(TargetTag tag)
    {
        if (enemyData == null || enemyData.targetTags == null)
            return false;

        return enemyData.targetTags.Contains(tag); // 태그 보유 확인
    }

    public bool HasElement(ElementType elementType)
    {
        if (enemyData == null)
            return false;

        if (enemyData.mainElement == elementType)
            return true; // 메인 속성 확인

        if (enemyData.subElements == null)
            return false;

        return enemyData.subElements.Contains(elementType); // 보조 속성 확인
    }

    public float GetResistanceMultiplier(ElementType elementType)
    {
        if (enemyData == null || enemyData.elementModifiers == null)
            return 1f; // 설정이 없으면 기본 배율

        foreach (ElementModifierData modifier in enemyData.elementModifiers)
        {
            if (modifier.elementType != elementType)
                continue;

            if (modifier.isImmune)
                return 0f; // 해당 속성 면역

            return modifier.damageMultiplier; // 속성 데미지 배율
        }

        return 1f; // 해당 속성 설정 없음
    }

    public int GetRankValue()
    {
        if (enemyData == null)
            return 0;

        return enemyData.rankValue; // 적 격 반환
    }
}