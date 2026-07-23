using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTreeController : MonoBehaviour
{
    public List<SkillTreeData> skillTrees = new(); // 전체 스킬트리 목록
    public SkillTreeData currentData;

    PlayerProgress playerProgress;
    RunState runState;




    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            playerProgress = GameManager.Instance.playerProgress;
            runState = GameManager.Instance.runState;
            return;
        }

        // 테스트 씬에서 GameManager가 없을 때 임시 데이터 생성
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
            AddPermanentSkill(node.skillData); // 스킬 데이터가 있으면 스킬풀에도 추가
        }

        Debug.Log("노드 해금: " + nodeId);

    }

    void AddPermanentSkill(SkillData skillData)
    {
        if (playerProgress == null || runState == null)
            return;

        if (skillData == null) return;

        if (!playerProgress.unlockedSkillPool.Contains(skillData.id)) //없으면 추가
            playerProgress.unlockedSkillPool.Add(skillData.id);

        if (!runState.availableSkillPool.Contains(skillData.id)) //없으면 추가
            runState.availableSkillPool.Add(skillData.id);



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
}
