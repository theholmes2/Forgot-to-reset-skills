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

    public GameObject tabViewObject; // 탭 목록 ScrollView
    public GameObject treeViewObject; // 스킬트리 ScrollView

    [Header("Tabs")]
    public RectTransform tabRoot; // 탭 버튼들이 생성될 부모 Content
    public SkillTreeTabButtonUI tabButtonPrefab; // 탭 버튼 프리팹

    private List<SkillTreeTabButtonUI> spawnedTabs = new(); // 생성된 탭 목록

    [Header("Equip Popup")]
    public SkillTreeEquipPopupUI equipPopup; // 해금된 스킬 장착 팝업

    [Header("Skill Point")]
    public TMPro.TMP_Text skillPointText;

    private void Start()
    {
        RefreshSkillPointText();
        CloseSkillTree(); // 시작할 때 닫아둠
    }

    private void OnEnable()
    {
        if (skillTreeController != null)
            skillTreeController.OnSkillPointsChanged += OnSkillPointsChanged;
    }

    private void OnDisable()
    {
        if (skillTreeController != null)
            skillTreeController.OnSkillPointsChanged -= OnSkillPointsChanged;
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
        if (equipPopup != null)
            equipPopup.Close(); // 트리 바뀔 때 팝업 닫기

        foreach (SkillTreeNodeUI nodeUI in spawnedNodes)
        {
            if (nodeUI != null)
                Destroy(nodeUI.gameObject);
        }

        spawnedNodes.Clear();
    }

    public void OnClickNode(string nodeId, RectTransform clickedNodeRect)
    {
        if (skillTreeController == null)
            return;
        if (!nodeMap.ContainsKey(nodeId))
            return;
        SkillTreeNodeData nodeData = nodeMap[nodeId]; // 클릭한 노드 데이터

        bool isUnlocked = skillTreeController.IsNodeUnlocked(nodeId); // 이미 해금됐는지 확인

        if (!isUnlocked)
        {
            if (!skillTreeController.CanUnlockNode(nodeId))
            {
                Debug.Log("해금 조건을 만족하지 못함: " + nodeId);

                if (GameSoundController.Instance != null)
                    GameSoundController.Instance.PlayUnlockDenied();

                RefreshAllNodes();
                return;
            }

            if (!skillTreeController.UnlockNode(nodeId))
            {
                RefreshAllNodes();
                return;
            }

            if (GameSoundController.Instance != null)
                GameSoundController.Instance.PlayNodeUnlock();

            RefreshAllNodes(); // 해금 상태 UI 갱신
            isUnlocked = true;
        }

        if (isUnlocked && nodeData.skillData != null && equipPopup != null)
        {
            equipPopup.Open(nodeData, clickedNodeRect); // 해금된 스킬이면 장착 팝업 열기
        }
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
            skillTreePanel.SetActive(true);

        if (tabViewObject != null)
            tabViewObject.SetActive(true); // 탭 화면 켜기

        if (treeViewObject != null)
            treeViewObject.SetActive(false); // 스킬트리 화면 끄기

        BuildTabs();
        RefreshSkillPointText();
        ClearTreeUI();
    }

    public void CloseSkillTree()
    {
        isOpen = false;

        ClearTabs();
        ClearTreeUI();

        if (tabViewObject != null)
            tabViewObject.SetActive(false);

        if (treeViewObject != null)
            treeViewObject.SetActive(false);

        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);
    }


    public void BuildTabs()
    {
        ClearTabs();

        if (skillTreeController == null)
        {
            Debug.Log("SkillTreeController 없음");
            return;
        }

        if (tabRoot == null)
        {
            Debug.Log("TabRoot 없음");
            return;
        }

        if (tabButtonPrefab == null)
        {
            Debug.Log("TabButtonPrefab 없음");
            return;
        }

        List<SkillTreeData> visibleTrees = skillTreeController.GetVisibleTrees();

        Debug.Log("보이는 스킬트리 개수: " + visibleTrees.Count);

        foreach (SkillTreeData tree in visibleTrees)
        {
            Debug.Log("탭 생성: " + tree.displayName);

            SkillTreeTabButtonUI tab = Instantiate(tabButtonPrefab, tabRoot);
            tab.Init(tree, this);
            spawnedTabs.Add(tab);
        }
    }

    private void ClearTabs()
    {
        foreach (SkillTreeTabButtonUI tab in spawnedTabs)
        {
            if (tab != null)
                Destroy(tab.gameObject);
        }

        spawnedTabs.Clear();
    }

    private void SelectFirstVisibleTree()
    {
        if (skillTreeController == null)
            return;

        List<SkillTreeData> visibleTrees = skillTreeController.GetVisibleTrees();

        if (visibleTrees.Count == 0)
        {
            ClearTreeUI(); // 보여줄 트리가 없으면 노드 비우기
            return;
        }

        SelectTree(visibleTrees[0]); // 첫 번째 탭 자동 선택
    }

    public void SelectTree(SkillTreeData treeData)
    {
        if (skillTreeController == null)
            return;

        if (tabViewObject != null)
            tabViewObject.SetActive(false); // 탭 화면 끄기

        if (treeViewObject != null)
            treeViewObject.SetActive(true); // 스킬트리 화면 켜기

        skillTreeController.SetCurrentTree(treeData);
        BuildTreeUI();
    }

    private void OnSkillPointsChanged(int skillPoints)
    {
        RefreshSkillPointText();
        RefreshAllNodes();
    }

    private void RefreshSkillPointText()
    {
        if (skillPointText == null || skillTreeController == null)
            return;

        skillPointText.text = "SP " + skillTreeController.GetSkillPoints();
    }

}
