using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeTabButtonUI : MonoBehaviour
{
    public Button button; // 탭 버튼
    public TMP_Text nameText; // 스킬트리 이름 표시
    public Image iconImage; // 스킬트리 대표 아이콘 표시

    private SkillTreeData treeData; // 이 버튼이 담당하는 스킬트리
    private SkillTreeUIManager uiManager; // 클릭 전달용

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClick); // 버튼 클릭 등록
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick); // 버튼 클릭 해제
    }

    public void Init(SkillTreeData data, SkillTreeUIManager manager)
    {
        treeData = data; // 스킬트리 저장
        uiManager = manager; // UI 매니저 저장

        if (nameText != null && treeData != null)
            nameText.text = treeData.displayName; // 버튼 이름 표시

        if (iconImage != null && treeData != null)
        {
            iconImage.sprite = treeData.icon; // 대표 아이콘 표시
            iconImage.enabled = treeData.icon != null;
        }
    }

    private void OnClick()
    {
        if (treeData == null || uiManager == null)
            return;

        uiManager.SelectTree(treeData); // 선택한 스킬트리 표시 요청
    }
}
