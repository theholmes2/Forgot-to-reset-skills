using System.Collections.Generic;
using UnityEngine;

public enum SkillTreeOwnerType
{
    Player,     // 플레이어 전용 트리
    Enemy,      // 적/NPC 트리
    Stolen,     // 훔친 스킬 트리
    Common      // 공용 트리
}

[CreateAssetMenu(fileName = "NewSkillTreeData", menuName = "Game/Skill Tree Data")]
public class SkillTreeData : ScriptableObject
{
    [Header("Tree Info")]
    public string id; // 스킬트리 ID
    public string displayName; // 스킬트리 이름
    public string description; // 스킬트리 설명

    [Header("Owner")]
    public SkillTreeOwnerType ownerType; // 누구의 스킬트리인지

    [Header("Nodes")]
    public List<SkillTreeNodeData> nodes = new(); // 스킬트리 노드 목록
}