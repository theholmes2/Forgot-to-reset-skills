using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeNodeUI : MonoBehaviour
{
    public Button iconButton; // 아이콘 자체를 버튼으로 사용
    public Image iconImage; // 스킬 아이콘 표시
    public Image lockCover; // 잠금 상태 덮개
    public TMP_Text nameText; // 노드 이름 표시
    public Image backgroundImage; // 상태 색 표시용 배경

    public Color unlockedColor = Color.white; // 해금된 노드 색
    public Color canUnlockColor = new Color(0.7f, 1f, 0.7f, 1f); // 해금 가능 색
    public Color lockedColor = new Color(0.45f, 0.45f, 0.45f, 1f); // 잠김 색

    private SkillTreeNodeData nodeData; // 이 UI가 표시하는 노드 데이터
    private SkillTreeUIManager uiManager; // 클릭을 전달할 UI 매니저

    private void Awake()
    {
        if (iconButton != null)
            iconButton.onClick.AddListener(OnClick); // 버튼 클릭 등록
    }

    private void OnDestroy()
    {
        if (iconButton != null)
            iconButton.onClick.RemoveListener(OnClick); // 버튼 클릭 해제
    }

    public void Init(SkillTreeNodeData node, SkillTreeUIManager manager)
    {
        nodeData = node; // 노드 데이터 저장
        uiManager = manager; // UI 매니저 저장

        Refresh(); // 처음 화면 갱신
    }

    public void Refresh()
    {
        if (nodeData == null)
            return;

        if (nameText != null)
            nameText.text = nodeData.displayName; // 노드 이름 표시

        RefreshIcon(); // 아이콘 표시
        RefreshState(); // 해금 상태 표시
    }

    private void RefreshIcon()
    {
        if (iconImage == null)
            return;

        Sprite icon = nodeData.iconOverride; // 노드 전용 아이콘 우선 사용

        if (icon == null && nodeData.skillData != null)
            icon = nodeData.skillData.icon; // 없으면 스킬 아이콘 사용

        iconImage.sprite = icon;
        // iconImage.enabled = icon != null; // 아이콘 없으면 비움
    }

    private void RefreshState()
    {
        if (uiManager == null)
            return;

        if (uiManager.IsNodeUnlocked(nodeData.nodeId))
        {
            SetUnlockedState(); // 이미 해금됨
            return;
        }

        if (uiManager.CanUnlockNode(nodeData.nodeId))
        {
            SetCanUnlockState(); // 지금 해금 가능
            return;
        }

        SetLockedState(); // 아직 잠김
    }

    private void OnClick()
    {
        if (nodeData == null || uiManager == null)
            return;

        // uiManager.OnClickNode(nodeData.nodeId); // 클릭한 노드 ID 전달

        RectTransform rect = GetComponent<RectTransform>(); // 클릭한 노드 위치
        uiManager.OnClickNode(nodeData.nodeId, rect); // 노드 ID + 위치 전달
    }

    private void SetUnlockedState()
    {
        if (lockCover != null)
            lockCover.gameObject.SetActive(false); // 해금되면 덮개 끔

        if (backgroundImage != null)
            backgroundImage.color = unlockedColor;
    }

    private void SetCanUnlockState()
    {
        if (lockCover != null)
            lockCover.gameObject.SetActive(false); // 해금 가능하면 덮개 끔

        if (backgroundImage != null)
            backgroundImage.color = canUnlockColor;
    }

    private void SetLockedState()
    {
        if (lockCover != null)
            lockCover.gameObject.SetActive(true); // 잠김이면 덮개 켬

        if (backgroundImage != null)
            backgroundImage.color = lockedColor;
    }
}
