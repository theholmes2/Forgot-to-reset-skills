using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTreeController : MonoBehaviour
{
    public List<SkillTreeData> skillTrees = new(); // 전체 스킬트리 목록
    public SkillTreeData currentData;

    [Header("Default Tree")]
    public SkillTreeData defaultSkillTree; // 새 게임부터 보이는 기본 스킬트리

    public event Action<int> OnSkillPointsChanged;

    PlayerProgress playerProgress;
    RunState runState;




    private void Start()
    {
        if (GameManager.Instance != null)
        {
            playerProgress = GameManager.Instance.playerProgress;
            runState = GameManager.Instance.runState;
        }
        else
        {
            Debug.LogWarning("GameManager가 없어 SkillTreeController가 임시 데이터를 사용합니다.");
            playerProgress = new PlayerProgress();
            runState = new RunState();
        }

        EnsureProgressLists();

        if (currentData == null)
            currentData = defaultSkillTree;

        EnsureDefaultStartNodesUnlocked();
    }
    public SkillTreeNodeData FindNode(string nodeId)
    {
        if (currentData == null) return null;

        foreach (SkillTreeNodeData node in currentData.nodes) //스킬트리 안 노드리스트 검사
        {
            if (node.nodeId == nodeId)
            { //같은 아이디 찾기
                return node;
            }
        }

        return null; //없으면 null
    }

    public bool IsNodeUnlocked(string nodeId)
    {
        if (playerProgress == null)
            return false;

        if (playerProgress.unlockedSkillNodeIds == null)
            return false;

        return playerProgress.unlockedSkillNodeIds.Contains(nodeId);


    }

    public bool CanUnlockNode(string nodeId)
    {
        SkillTreeNodeData findNode = FindNode(nodeId); //노드 찾고


        if (findNode == null)
            return false;

        if (IsNodeUnlocked(nodeId))
            return false; // 이미 해금된 노드는 다시 해금 불가

        if (!HasEnoughSkillPoint(findNode))
            return false;

        if (findNode.isStartNode == true)  //1.시작노드면 true
        {
            return true;
        }
        else if (findNode.requireAllRequiredNodes == true) //2.선행 노드 전체 해금 필요시
        {
            return HasAllRequiredNodes(findNode);

        }
        else if (findNode.requireAllRequiredNodes == false) //3.선행 노드 일부만 해금 필요시
        {
            return HasRequiredNodes(findNode);
        }



        return false;
    }
    bool HasAllRequiredNodes(SkillTreeNodeData node) //선행노드 해금확인
    {
        foreach (string nodeId in node.requiredNodeIds)
        { //부모노드 하나씩
            if (IsNodeUnlocked(nodeId) == false)
            {
                return false;
            }

        }

        return true;
    }
    bool HasRequiredNodes(SkillTreeNodeData node) //선행노드 해금확인
    {
        foreach (string nodeId in node.requiredNodeIds)
        { //부모노드 하나씩
            if (IsNodeUnlocked(nodeId) == true)
            {
                return true;
            }

        }

        return false;
    }

    bool HasEnoughSkillPoint(SkillTreeNodeData node)
    {
        if (playerProgress == null || node == null)
            return false;

        return playerProgress.skillPoints >= Mathf.Max(0, node.needSkillPoint);
    }

    public bool UnlockNode(string nodeId)
    {
        SkillTreeNodeData node = FindNode(nodeId); // 노드 찾기

        if (node == null || !CanUnlockNode(nodeId))
            return false;

        playerProgress.skillPoints -= Mathf.Max(0, node.needSkillPoint);
        playerProgress.unlockedSkillNodeIds.Add(nodeId); // 해금한 노드 기록

        if (node.skillData != null)
        {
            AddPermanentSkill(node.skillData, false);
            // 노드 해금 중에 스킬도 추가
            // 저장은 아래에서 한 번만 처리
        }

        SaveSystem.Save(playerProgress); // 내가 찍은 스킬 노드는 즉시 자동저장
        OnSkillPointsChanged?.Invoke(playerProgress.skillPoints);

        Debug.Log("노드 해금: " + nodeId);
        return true;
    }

    public bool AddPermanentSkill(SkillData skillData, bool autoSave = true)
    {
        if (playerProgress == null || runState == null)
            return false;

        if (skillData == null)
            return false;

        bool isChanged = false; // 실제로 새로 추가됐는지 확인

        if (!playerProgress.unlockedSkillPool.Contains(skillData.id))
        {
            playerProgress.unlockedSkillPool.Add(skillData.id); // 영구 스킬 목록에 추가
            isChanged = true;
        }

        if (!runState.availableSkillPool.Contains(skillData.id))
        {
            runState.availableSkillPool.Add(skillData.id); // 이번 회차 사용 가능 목록에도 추가
            isChanged = true;
        }

        if (isChanged && autoSave)
        {
            SaveSystem.Save(playerProgress); // 보스 보상 같은 외부 영구 해금도 즉시 저장
        }

        return isChanged;
    }

    List<SkillTreeNodeData> GetChildNodes(string nodeId) //부모 아이디 입력
    {
        List<SkillTreeNodeData> children = new List<SkillTreeNodeData>(); //리턴용 리스트



        foreach (SkillTreeNodeData skillTreeNodeData in currentData.nodes) //현제 대상인 스킬트리의 노드들을 전부 확인
        {
            foreach (string parentNodeIds in skillTreeNodeData.requiredNodeIds) //해당 노드한개의 부모노드 전부 확인
            {
                if (nodeId == parentNodeIds) //부모아이디와 찾는게 같다면
                {
                    children.Add(skillTreeNodeData); //리스트에 추가
                }
            }
        }

        return children;
    }

    public void SetCurrentTree(SkillTreeData treeData)
    {
        currentData = treeData; // 현재 보고 있는 스킬트리 변경
    }

    public List<SkillTreeData> GetVisibleTrees()
    {
        List<SkillTreeData> visibleTrees = new List<SkillTreeData>();

        foreach (SkillTreeData tree in skillTrees)
        {
            if (tree == null)
                continue;

            if (CanShowTree(tree))
                visibleTrees.Add(tree); // 보여줄 수 있는 트리만 추가
        }

        return visibleTrees;
    }

    private bool CanShowTree(SkillTreeData tree)
    {
        if (tree == null)
            return false;

        if (tree == defaultSkillTree)
            return true;

        return playerProgress != null && playerProgress.unlockedSkillTreeIds.Contains(tree.id);
    }

    public bool UnlockSkillTree(SkillTreeData tree, bool autoSave = true)
    {
        if (tree == null || playerProgress == null || string.IsNullOrEmpty(tree.id))
            return false;

        if (tree == defaultSkillTree || playerProgress.unlockedSkillTreeIds.Contains(tree.id))
            return false;

        playerProgress.unlockedSkillTreeIds.Add(tree.id);

        if (autoSave)
            SaveSystem.Save(playerProgress);

        return true;
    }

    public int GetSkillPoints()
    {
        return playerProgress != null ? playerProgress.skillPoints : 0;
    }

    public bool AddSkillPoints(int amount, bool autoSave = true)
    {
        if (playerProgress == null || amount <= 0)
            return false;

        playerProgress.skillPoints += amount;

        if (autoSave)
            SaveSystem.Save(playerProgress);

        OnSkillPointsChanged?.Invoke(playerProgress.skillPoints);
        return true;
    }

    public bool GrantSkillPointReward(string rewardId, int amount)
    {
        if (playerProgress == null || string.IsNullOrEmpty(rewardId) || amount <= 0)
            return false;

        if (playerProgress.receivedRewardIds.Contains(rewardId))
            return false;

        playerProgress.receivedRewardIds.Add(rewardId);
        playerProgress.skillPoints += amount;
        SaveSystem.Save(playerProgress);
        OnSkillPointsChanged?.Invoke(playerProgress.skillPoints);
        return true;
    }

    private void EnsureProgressLists()
    {
        playerProgress.unlockedAbilityIds ??= new List<string>();
        playerProgress.achievementIds ??= new List<string>();
        playerProgress.unlockedSkillPool ??= new List<string>();
        playerProgress.unlockedSkillNodeIds ??= new List<string>();
        playerProgress.unlockedSkillTreeIds ??= new List<string>();
        playerProgress.receivedRewardIds ??= new List<string>();
    }

    private void EnsureDefaultStartNodesUnlocked()
    {
        if (defaultSkillTree == null || defaultSkillTree.nodes == null)
            return;

        bool hasChanged = false;

        foreach (SkillTreeNodeData node in defaultSkillTree.nodes)
        {
            if (node == null || !node.isStartNode)
                continue;

            if (!playerProgress.unlockedSkillNodeIds.Contains(node.nodeId))
            {
                playerProgress.unlockedSkillNodeIds.Add(node.nodeId);
                hasChanged = true;
            }

            if (node.skillData != null && AddPermanentSkill(node.skillData, false))
                hasChanged = true;
        }

        if (hasChanged && GameManager.Instance != null)
            SaveSystem.Save(playerProgress);
    }

    private SkillTreeNodeData FindNodeBySkill(SkillData skillData)
    {
        if (skillData == null)
            return null;

        foreach (SkillTreeData tree in skillTrees)
        {
            if (tree == null || tree.nodes == null)
                continue;

            foreach (SkillTreeNodeData node in tree.nodes)
            {
                if (node == null || node.skillData == null)
                    continue;

                // ScriptableObject 참조가 같으면 해당 스킬 노드
                if (node.skillData == skillData)
                    return node;

                // 혹시 다른 인스턴스라도 ID가 같으면 같은 스킬로 판단
                if (!string.IsNullOrEmpty(skillData.id) &&
                    node.skillData.id == skillData.id)
                {
                    return node;
                }
            }
        }

        return null;
    }

    public bool GrantPermanentSkillReward(
    SkillData skillData,
    bool autoSave = true)
    {
        if (playerProgress == null || runState == null)
            return false;

        if (skillData == null)
            return false;

        // 지급하기 전에 이미 영구 스킬을 보유하고 있었는지 확인
        bool wasPermanentlyUnlocked =
            playerProgress.unlockedSkillPool.Contains(skillData.id);

        bool dataChanged = AddPermanentSkill(skillData, false);

        SkillTreeNodeData node = FindNodeBySkill(skillData);

        if (node != null &&
            !playerProgress.unlockedSkillNodeIds.Contains(node.nodeId))
        {
            // 기존 저장 데이터에 노드 해금만 빠져 있다면 함께 복구
            playerProgress.unlockedSkillNodeIds.Add(node.nodeId);
            dataChanged = true;
        }

        if (node == null)
        {
            Debug.LogWarning(
                "보상 스킬에 해당하는 스킬트리 노드를 찾지 못했습니다: "
                + skillData.id
            );
        }

        if (dataChanged && autoSave)
        {
            SaveSystem.Save(playerProgress);
        }

        // 실제 데이터 변경 여부가 아니라 최초 영구 획득 여부를 반환
        return !wasPermanentlyUnlocked;
    }
}
