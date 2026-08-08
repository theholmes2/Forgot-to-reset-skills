using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string id; // 적 ID
    public string displayName; // 적 이름

    public BaseStatData baseStats; // 기본 스탯
    public int rankValue; // 적 격

    public ElementType mainElement = ElementType.None; // 메인 속성
    public List<ElementType> subElements = new(); // 보조 속성
    public List<TargetTag> targetTags = new(); // 적 태그
    public List<ElementModifierData> elementModifiers = new(); // 속성 약점/저항 목록
}
