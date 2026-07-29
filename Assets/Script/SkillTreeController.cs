using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTreeController : MonoBehaviour
{
    public List<SkillTreeData> skillTrees = new(); // 전체 스킬트리 목록
    public SkillTreeData currentData;

    PlayerProgress playerProgress;
    RunState runState;




    private void Start()
    {
        if (GameManager.Instance != null)
        {
            playerProgress = GameManager.Instance.playerProgress;
            runState = GameManager.Instance.runState;
            return;
        }
        // GameManager가 없는 단독 테스트 씬에서만 임시 데이터 사용
        Debug.LogWarning("GameManager가 없어 SkillTreeController가 임시 데이터를 사용합니다.");
        
        playerProgress = new PlayerProgress();
        runState = new RunState();
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
        return false;
    }

    public void UnlockNode(string nodeId)
    {
        SkillTreeNodeData node = FindNode(nodeId); // 노드 찾기

        if (node == null)
            return;

        if (IsNodeUnlocked(nodeId))
            return; // 이미 해금된 노드면 종료

        playerProgress.unlockedSkillNodeIds.Add(nodeId); // 해금한 노드 기록

        if (node.skillData != null)
        {
            AddPermanentSkill(node.skillData, false);
            // 노드 해금 중에 스킬도 추가
            // 저장은 아래에서 한 번만 처리
        }

        SaveSystem.Save(playerProgress); // 내가 찍은 스킬 노드는 즉시 자동저장

        Debug.Log("노드 해금: " + nodeId);
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

        foreach (SkillTreeNodeData node in tree.nodes)
        {
            if (node == null)
                continue;

            if (IsNodeUnlocked(node.nodeId))
                return true; // 이미 하나라도 해금했으면 표시

            if (CanUnlockNodeInTree(tree, node.nodeId))
                return true; // 하나라도 해금 가능하면 표시
        }

        return false; // 해금된 것도, 해금 가능한 것도 없으면 숨김
    }

    private bool CanUnlockNodeInTree(SkillTreeData tree, string nodeId)
    {
        SkillTreeData beforeTree = currentData; // 원래 보고 있던 트리 저장

        currentData = tree; // 검사할 트리로 잠깐 변경
        bool result = CanUnlockNode(nodeId); // 기존 해금 가능 검사 재사용
        currentData = beforeTree; // 다시 원래 트리로 복구

        return result;
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

        bool isChanged = AddPermanentSkill(skillData, false);

        SkillTreeNodeData node = FindNodeBySkill(skillData);

        if (node != null &&
            !playerProgress.unlockedSkillNodeIds.Contains(node.nodeId))
        {
            playerProgress.unlockedSkillNodeIds.Add(node.nodeId);
            isChanged = true;
        }

        if (node == null)
        {
            Debug.LogWarning(
                "보상 스킬에 해당하는 스킬트리 노드를 찾지 못했습니다: "
                + skillData.id
            );
        }

        if (isChanged && autoSave)
        {
            SaveSystem.Save(playerProgress);
        }

        return isChanged;
    }
}
