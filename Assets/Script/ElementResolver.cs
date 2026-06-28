using UnityEngine;

public class ElementResolver : MonoBehaviour
{
    public float GetElementMultiplier(ElementType attackElement, EnemyTraitController target)
    {
        // TODO:
        // 1. target의 elementModifiers 조회
        // 2. 같은 속성의 damageMultiplier 반환
        // 3. 없으면 1 반환
        return 1f;
    }

    public bool IsElementBlocked(ElementType userElement, ElementType skillElement)
    {
        // TODO:
        // 1. 예: Light면 Dark 스킬 사용 불가
        // 2. 예: 특정 패시브/장비 때문에 금지
        return false;
    }

    public float ApplyElementPenalty(float value, ElementType skillElement, EnemyTraitController target)
    {
        // TODO:
        // 1. GetElementMultiplier 호출
        // 2. value * multiplier 반환
        return value;
    }

    public bool CheckElementConflict(ElementType elementA, ElementType elementB)
    {
        // TODO:
        // 1. 두 속성이 서로 충돌/금지 관계인지 판단
        return false;
    }
}
