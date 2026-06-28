using System;
using System.Collections.Generic;
using UnityEngine;

#region Enums

public enum StatType
{
    Attack,         // 공격력
    Defense,        // 방어력
    MaxHp,          // 최대 체력
    MaxMp,          // 최대 마나
    MoveSpeed,      // 이동속도
    KnockBack,      // 넉백 힘
    CritRate,       // 치명타 확률
    CritDamage,     // 치명타 피해
    CastSpeed,      // 시전속도
    CooldownRate    // 쿨타임 감소율
}

public enum ModifierType
{
    Flat,           // 고정값 더하기
    PercentAdd,     // % 더하기
    PercentMul,     // 최종 배율 곱하기
    Override,       // 값 덮어쓰기
    ClampMin,       // 최소 제한
    ClampMax        // 최대 제한
}

public enum TriggerType
{
    PassiveAlways,  // 항상 적용
    OnEquip,        // 장착 시
    OnUnequip,      // 해제 시
    OnAttack,       // 공격 시
    OnHit,          // 적중 시
    OnDamaged,      // 피격 시
    OnKill,         // 처치 시
    OnSkillCast,    // 스킬 사용 시
    OnJump          // 점프 시
}

public enum ElementType
{
    None,
    Fire,           // 불
    Water,          // 물
    Wind,           // 바람
    Earth,          // 땅
    Light,          // 빛
    Dark,           // 어둠
    Poison,         // 독
    Holy,           // 성
    Arcane          // 마법/비전
}

public enum TargetTag
{
    Human,          // 인간
    Dragon,         // 용
    PlantSpirit,    // 풀 정령
    Undead,         // 언데드
    Demon,          // 악마
    Boss,           // 보스
    Flying,         // 비행
    Armored,        // 중장갑
    Beast           // 야수
}

public enum RestrictionResultType
{
    Allow,          // 허용
    Penalty,        // 패널티 후 허용
    Block           // 사용 불가
}

public enum EquipmentSlotType
{
    Weapon,
    Armor,
    Accessory,
    JumpSlot        // 점프 전용 슬롯
}

#endregion

#region Base Data

[Serializable]
public class BaseStatData
{
    public float attack = 10f;          // 기본 공격력
    public float defense = 5f;          // 기본 방어력
    public float maxHp = 100f;          // 기본 최대 체력
    public float maxMp = 50f;           // 기본 최대 마나
    public float moveSpeed = 3f;        // 기본 이동속도
    public float knockBack = 15f;       // 기본 넉백 힘
    public float critRate = 0f;         // 기본 치명타 확률
    public float critDamage = 1.5f;     // 기본 치명타 피해
    public float castSpeed = 1f;        // 기본 시전 속도
    public float cooldownRate = 1f;     // 기본 쿨타임 배율
}

[Serializable]
public class RestrictionRuleData
{
    public int minimumUserRank = 0;                         // 최소 사용자 격
    public int minimumTargetRank = 0;                       // 최소 대상 격

    public List<ElementType> allowedUserElements = new();   // 허용 사용자 속성
    public List<ElementType> blockedUserElements = new();   // 금지 사용자 속성

    public List<ElementType> allowedSkillElements = new();  // 허용 스킬 속성
    public List<ElementType> blockedSkillElements = new();  // 금지 스킬 속성

    public List<ElementType> allowedEquipmentElements = new(); // 허용 장비 속성
    public List<ElementType> blockedEquipmentElements = new(); // 금지 장비 속성

    public List<TargetTag> requiredTargetTags = new();      // 필요한 상대 태그
    public List<TargetTag> blockedTargetTags = new();       // 금지 상대 태그

    public RestrictionResultType resultType = RestrictionResultType.Allow; // 검사 결과 타입
    public float penaltyMultiplier = 1f;                    // 패널티 배율
    public string failureMessageKey;                        // UI 메시지용 키
}

[Serializable]
public class ElementModifierData
{
    public ElementType elementType;         // 대상 속성
    public float damageMultiplier = 1f;     // 피해 배율
    public float statusMultiplier = 1f;     // 상태이상 배율
    public bool isImmune = false;           // 완전 면역 여부
}

[Serializable]
public class EffectData
{
    public string id;                       // 효과 고유 ID
    public string displayName;              // 효과 표시 이름

    public TriggerType triggerType;         // 언제 발동되는지
    public StatType targetStat;             // 어떤 스탯을 바꾸는지
    public ModifierType modifierType;       // 어떤 방식으로 바꾸는지
    public float value;                     // 실제 값

    public float duration = 0f;             // 지속 시간
    public bool isPermanent; // 시간이 지나도 자동 제거하지 않음
    public float interval = 0f;             // 주기 효과 간격

    public ElementType elementType = ElementType.None; // 효과 자체 속성

    public bool ignoreRank = false;         // 격 무시 여부
    public int rankPenetrationValue = 0;    // 일부 격 무시값

    public List<TargetTag> requiredTargetTags = new();   // 반드시 필요한 대상 태그
    public List<TargetTag> forbiddenTargetTags = new();  // 있으면 안 되는 대상 태그

    public List<ElementType> requiredUserElements = new();   // 사용자 필요 속성
    public List<ElementType> forbiddenUserElements = new();  // 사용자 금지 속성

    public List<string> requiredPassiveIds = new();      // 필요한 패시브 ID
    public List<string> forbiddenPassiveIds = new();     // 금지 패시브 ID

    public RestrictionResultType restrictionResultType = RestrictionResultType.Allow; // 적용 실패 시 처리
    public float penaltyMultiplier = 1f;                 // 패널티 적용 시 배율

    public bool canAffectBoss = true;        // 보스에게 적용 가능한가
    public bool canAffectFlying = true;      // 비행 적에게 적용 가능한가
}

#endregion

#region ScriptableObject Data



[CreateAssetMenu(fileName = "NewEquipmentData", menuName = "Game/Equipment Data")]
public class EquipmentData : ScriptableObject
{
    public string id;                           // 장비 ID
    public string displayName;                  // 장비 이름
    public string description;                  // 설명
    public Sprite icon;                         // 아이콘

    public EquipmentSlotType slotType;          // 장비 슬롯 위치
    public ElementType equipmentElement;        // 장비 속성
    public int requiredRank = 0;                // 요구 격

    public List<EffectData> grantedEffects = new();       // 장비가 주는 효과
    public List<RestrictionRuleData> restrictionRules = new(); // 장착 제한 규칙

    public List<string> tags = new();           // 장비 전용 태그
    public bool isExclusiveWithOtherElements;   // 다른 속성과 배타적인가
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string id;                       // 적 ID
    public string displayName;              // 적 이름

    public BaseStatData baseStats;          // 기본 스탯
    public int rankValue = 0;               // 적 격

    public ElementType mainElement = ElementType.None; // 메인 속성
    public List<ElementType> subElements = new();      // 보조 속성
    public List<TargetTag> targetTags = new();         // 적 태그
    public List<ElementModifierData> elementModifiers = new(); // 속성 약점/저항 목록
}

[CreateAssetMenu(fileName = "NewPlayerTraitData", menuName = "Game/Player Trait Data")]
public class PlayerTraitData : ScriptableObject
{
    public ElementType mainElement = ElementType.None; // 플레이어 주 속성
    public List<ElementType> subElements = new();      // 보조 속성
    public int rankValue = 0;                          // 플레이어 격

    public List<string> passiveIds = new();            // 보유 패시브 ID
    public List<string> traitTags = new();             // 플레이어 태그
    public List<RestrictionRuleData> restrictionRules = new(); // 플레이어 제한 규칙
}

#endregion
