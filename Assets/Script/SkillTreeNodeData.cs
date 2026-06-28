using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatRequirementData
{
    public StatType statType; // 필요한 스탯 종류
    public float needValue; // 필요한 스탯 수치
}

[System.Serializable]
public class SkillTreeNodeData
{
   

    [Header("Node Info")]
    public string nodeId; // 스킬트리 안에서 쓰는 노드 ID
    public string displayName; // 노드 표시 이름
    public string description; // 노드 설명

    [Header("Skill")]
    public SkillData skillData; // 이 노드가 해금하는 스킬
    public Sprite iconOverride; // 스킬 아이콘 대신 쓸 아이콘, 없으면 SkillData.icon 사용

    [Header("Tree Position")]
    public Vector2 uiPosition; // 나중에 스킬트리 UI에서 노드 배치용

    [Header("Unlock Cost")]
    public int needSkillPoint = 1; // 필요한 스킬 포인트
    public int needPlayerLevel = 0; // 필요한 플레이어 레벨
    public int needRank = 0; // 필요한 격

    [Header("Prerequisite Nodes")]
    public bool isStartNode; // 시작부터 열 수 있는 노드인지
    public bool requireAllRequiredNodes = true; // 선행 노드를 전부 요구할지
    public List<string> requiredNodeIds = new(); // 먼저 해금되어야 하는 노드 ID들

    [Header("Extra Requirements")]
    public List<StatRequirementData> statRequirements = new(); // 필요한 스탯 조건
    public List<string> requiredQuestIds = new(); // 필요한 퀘스트 완료 ID
    public List<string> requiredAchievementIds = new(); // 필요한 업적 ID

    [Header("Future Options")]
    public bool canBeStolen = true; // 훔쳐서 해금 가능한 스킬인지
    public bool canBeTemporaryUnlocked = true; // 일시 해금 가능한 노드인지
    public bool canBeForceUnlocked = true; // 이벤트/퀘스트로 강제 해금 가능한 노드인지
}