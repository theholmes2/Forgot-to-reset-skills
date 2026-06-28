
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    public string id;               // 내부 저장용 ID
    public string displayName;      // 실제 게임에 보이는 이름
    public SkillCategory category;   // 큰 분류 : 패시브 액티브
    public string description;      // 설명
    public Sprite icon;             // 아이콘

    public bool isUnLock;   // 해금여부

    
    public float damageMultiplier = 1f; //데미지 배율
    public float range = 1f;

    public float cooldown = 1f;        //쿨타임
    public int manaCost = 10;           // 마나소모량
    public float castTime = 0f;             // 시전 시간

    public GameObject skillPrefab; //스킬 프리팹
    public SkillSpawnType spawnType;  //생성위치
    public SkillAnimationType animationType; //애니메이션 타입
    public bool isBuffSkill; // 버프 스킬인지
   

    public ElementType skillElement;        // 스킬 속성
    public int requiredRank = 0;            // 필요 격

    public List<EffectData> effects = new(); // 스킬이 가진 효과 목록
    public List<RestrictionRuleData> restrictionRules = new(); // 스킬 사용 제한 규칙

}
