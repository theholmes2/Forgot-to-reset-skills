
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

    public float damage = 1f;
    public float cooldown = 1f;
    public float range = 1f;
    public int needMana = 10;           // 마나소모량

    public GameObject skillPrefab;
    public SkillSpawnType spawnType;
    public SkillAnimationType animationType;
}


