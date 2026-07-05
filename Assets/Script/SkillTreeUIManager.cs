using System.Collections.Generic;
using UnityEngine;

public class SkillTreeUIManager : MonoBehaviour
{
    public SkillTreeController skillTreeController; // 스킬트리 해금 로직 담당
    public RectTransform nodeContainer; // Scroll View의 Content
    public SkillTreeNodeUI nodePrefab; // 노드 UI 프리팹

    public float nodeSpacingX = 150f; // 같은 계층 안 노드 간격
    public float nodeSpacingY = 140f; // 계층 간 세로 간격
    public Vector2 contentPadding = new Vector2(200f, 120f); // 스크롤 여백
    private float contentHeight;

    private List<SkillTreeNodeUI> spawnedNodes = new(); // 생성된 노드 UI 목록
    private Dictionary<string, SkillTreeNodeData> nodeMap = new(); // nodeId로 노드 찾기
    private Dictionary<string, int> depthMap = new(); // nodeId별 계층 깊이

    public GameObject skillTreePanel; // 스킬트리 전체 패널
    private bool isOpen; // 현재 열려있는지

    private void Start()
    {
       
        BuildTreeUI(); // 시작 시 스킬트리 UI 생성
        CloseSkillTree(); // 시작할 때 닫아둠
    }

    public void BuildTreeUI()
    {
        ClearTreeUI(); // 기존 노드 정리

        if (skillTreeController == null) return;
        if (skillTreeController.currentData == null) return;
        if (nodeContainer == null || nodePrefab == null) return;

        BuildNodeMap(); // 노드 검색용 딕셔너리 생성
        BuildDepthMap(); // 노드 계층 계산

        Dictionary<int, List<SkillTreeNodeData>> depthGroups = BuildDepthGroups(); // 계층별 노드 묶기

        ResizeContent(depthGroups); // 스크롤 영역 크기 조정

        foreach (var group in depthGroups)
        {
            CreateDepthNodes(group.Key, group.Value); // 계층별로 노드 생성
        }
        
     
        RefreshAllNodes(); // 생성 후 상태 갱신
    }

    private void BuildNodeMap()
    {
        nodeMap.Clear();

        foreach (SkillTreeNodeData node in skillTreeController.currentData.nodes)
        {
            if (node == null) continue;
            if (string.IsNullOrEmpty(node.nodeId)) continue;
            if (nodeMap.ContainsKey(node.nodeId)) continue;

            nodeMap.Add(node.nodeId, node); // nodeId로 노드 저장
        }
    }

    private void BuildDepthMap()
    {
        depthMap.Clear();

        foreach (SkillTreeNodeData node in skillTreeController.currentData.nodes)
        {
            CalculateDepth(node); // 모든 노드의 계층 계산
        }
    }

    private int CalculateDepth(SkillTreeNodeData node)
    {
        if (node == null) return 0;

        if (depthMap.ContainsKey(node.nodeId))
            return depthMap[node.nodeId]; // 이미 계산된 계층 반환

        if (node.isStartNode || node.requiredNodeIds == null || node.requiredNodeIds.Count == 0)
        {
            depthMap[node.nodeId] = 0; // 시작 노드는 0계층
            return 0;
        }

        int maxParentDepth = 0;

        foreach (string parentId in node.requiredNodeIds)
        {
            if (!nodeMap.ContainsKey(parentId))
                continue;

            SkillTreeNodeData parentNode = nodeMap[parentId]; // 부모 노드 찾기
            int parentDepth = CalculateDepth(parentNode); // 부모 계층 계산

            if (parentDepth > maxParentDepth)
                maxParentDepth = parentDepth; // 가장 깊은 부모 기준
        }

        int depth = maxParentDepth + 1; // 부모보다 한 단계 아래
        depthMap[node.nodeId] = depth;

        return depth;
    }

    private Dictionary<int, List<SkillTreeNodeData>> BuildDepthGroups()
    {
        Dictionary<int, List<SkillTreeNodeData>> depthGroups = new();

        foreach (SkillTreeNodeData node in skillTreeController.currentData.nodes)
        {
            int depth = depthMap[node.nodeId];

            if (!depthGroups.ContainsKey(depth))
                depthGroups.Add(depth, new List<SkillTreeNodeData>());

            depthGroups[depth].Add(node); // 같은 계층끼리 묶기
        }

        return depthGroups;
    }

    private void CreateDepthNodes(int depth, List<SkillTreeNodeData> nodes)
    {
        int count = nodes.Count;

        for (int i = 0; i < count; i++)
        {
            SkillTreeNodeData node = nodes[i];
            SkillTreeNodeUI nodeUI = Instantiate(nodePrefab, nodeContainer); // Content 아래 생성

            RectTransform rect = nodeUI.GetComponent<RectTransform>();

            if (rect != null)
            {
                float x = (i - (count - 1) * 0.5f) * nodeSpacingX; // 가운데 정렬
                float y = (contentHeight * 0.5f) - contentPadding.y - depth * nodeSpacingY; // 계층이 깊을수록 아래로

                rect.anchoredPosition = new Vector2(x, y);
            }

            nodeUI.Init(node, this); // 노드 데이터 연결
            spawnedNodes.Add(nodeUI); // 생성 목록 저장
        }
    }

    private void ResizeContent(Dictionary<int, List<SkillTreeNodeData>> depthGroups)
    {
        int maxDepth = 0;
        int maxCount = 1;

        foreach (var group in depthGroups)
        {
            if (group.Key > maxDepth)
                maxDepth = group.Key;

            if (group.Value.Count > maxCount)
                maxCount = group.Value.Count;
        }

        float width = maxCount * nodeSpacingX + contentPadding.x * 2f;
        float height = (maxDepth + 1) * nodeSpacingY + contentPadding.y * 2f;

        contentHeight = height; // Content 높이 저장

        nodeContainer.sizeDelta = new Vector2(width, height); // Scroll View가 움직일 영역
        

    }

    public void ClearTreeUI()
    {
        foreach (SkillTreeNodeUI nodeUI in spawnedNodes)
        {
            if (nodeUI != null)
                Destroy(nodeUI.gameObject);
        }

        spawnedNodes.Clear();
    }

    public void OnClickNode(string nodeId)
    {
        if (skillTreeController == null)
            return;

        if (!skillTreeController.CanUnlockNode(nodeId))
        {
            Debug.Log("해금 조건을 만족하지 못함: " + nodeId);
            RefreshAllNodes();
            return;
        }

        skillTreeController.UnlockNode(nodeId); // 해금 요청
        RefreshAllNodes(); // UI 갱신
    }

    public void RefreshAllNodes()
    {
        foreach (SkillTreeNodeUI nodeUI in spawnedNodes)
        {
            if (nodeUI != null)
                nodeUI.Refresh();
        }
    }

    public bool IsNodeUnlocked(string nodeId)
    {
        if (skillTreeController == null)
            return false;

        return skillTreeController.IsNodeUnlocked(nodeId);
    }

    public bool CanUnlockNode(string nodeId)
    {
        if (skillTreeController == null)
            return false;

        return skillTreeController.CanUnlockNode(nodeId);
    }



    public void ToggleSkillTree()
    {
        if (isOpen)
        {
            CloseSkillTree(); // 열려 있으면 닫기
        }
        else
        {
            OpenSkillTree(); // 닫혀 있으면 열기
        }
    }

    public void OpenSkillTree()
    {
        isOpen = true;

        if (skillTreePanel != null)
            skillTreePanel.SetActive(true); // 패널 켜기

        BuildTreeUI(); // 열릴 때 UI 갱신
    }

    public void CloseSkillTree()
    {
        isOpen = false;

        if (skillTreePanel != null)
            skillTreePanel.SetActive(false); // 패널 끄기
    }

}