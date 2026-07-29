using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeEquipPopupUI : MonoBehaviour
{
    [System.Serializable]
    public class EquipSlotButton
    {
        public SkillSlotKey key; // J/L/U/I/O 중 어떤 슬롯인지
        public Button button; // 슬롯 버튼
        public Image iconImage; // 현재 장착된 스킬 아이콘
        public TMP_Text keyText; // J/L/U/I/O 표시

        public void Refresh(PlayerSkillController controller, Sprite emptySlotIcon)
        {
            if (keyText != null)
                keyText.text = key.ToString();

            SkillSlot slot = controller.GetSlot(key);

            if (iconImage == null)
                return;

            if (slot == null || slot.skillData == null)
            {
                iconImage.sprite = emptySlotIcon; // 빈 슬롯 기본 이미지
                iconImage.enabled = emptySlotIcon != null;
                return;
            }

            iconImage.sprite = slot.skillData.icon;
            iconImage.enabled = slot.skillData.icon != null;
        }
    }

    public RectTransform popupRect; // SkillEquipPopup 자신의 RectTransform
    public Vector2 popupOffset = new Vector2(0f, -110f); // 노드 밑으로 띄울 거리

    public PlayerSkillController playerSkillController; // 실제 장착 처리 담당

    public Sprite emptySlotIcon; // 빈 슬롯 기본 아이콘
    public Image selectedSkillIcon; // 왼쪽 큰 아이콘
    public TMP_Text selectedSkillNameText; // 선택 스킬 이름
    public TMP_Text selectedSkillDescriptionText; // 선택 스킬 설명

    public Button closeButton; // 닫기 버튼
    public List<EquipSlotButton> slotButtons; // J/L/U/I/O 버튼들

    private SkillData selectedSkillData; // 지금 장착하려고 선택한 스킬

    public ScrollRect descriptionScrollRect;

    private void Awake()
    {
        if (popupRect == null)
            popupRect = GetComponent<RectTransform>();

        if (popupRect != null)
        {
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 1f); // 위쪽 기준으로 아래로 펼쳐지게
        }

        if (playerSkillController == null)
            playerSkillController = FindAnyObjectByType<PlayerSkillController>();

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        foreach (EquipSlotButton slotButton in slotButtons)
        {
            SkillSlotKey key = slotButton.key;

            if (slotButton.button != null)
                slotButton.button.onClick.AddListener(() => EquipSelectedSkill(key));
        }

     
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Close(); // ESC로 닫기
    }

    public void Open(SkillTreeNodeData nodeData, RectTransform clickedNodeRect)
    {
        if (nodeData == null || nodeData.skillData == null)
            return;

        selectedSkillData = nodeData.skillData; // 장착할 스킬 저장

        gameObject.SetActive(true);

        if (clickedNodeRect != null && popupRect != null)
        {
            popupRect.SetAsLastSibling(); // 노드보다 위에 보이게 함

        }

        RefreshSelectedSkillInfo(); // 왼쪽/오른쪽 정보 표시
        RefreshSlots(); // 현재 장착 슬롯 표시
    }

    public void Close()
    {
        selectedSkillData = null;
        gameObject.SetActive(false);
    }

    private void EquipSelectedSkill(SkillSlotKey key)
    {
        if (selectedSkillData == null)
            return;

        if (playerSkillController == null)
            return;

        playerSkillController.SetSkill(key, selectedSkillData); // 실제 장착

        RefreshSlots(); // 슬롯 아이콘 갱신
        Close(); // 장착 후 닫기
    }

    private void RefreshSelectedSkillInfo()
    {
        if (selectedSkillData == null)
            return;

        if (selectedSkillIcon != null)
        {
            selectedSkillIcon.sprite = selectedSkillData.icon;
            selectedSkillIcon.enabled = selectedSkillData.icon != null;
        }

        if (selectedSkillNameText != null)
            selectedSkillNameText.text = selectedSkillData.displayName;

        if (selectedSkillDescriptionText != null)
            selectedSkillDescriptionText.text = selectedSkillData.description;

        if (descriptionScrollRect != null)
            descriptionScrollRect.verticalNormalizedPosition = 1f;
    }

    private void RefreshSlots()
    {
        foreach (EquipSlotButton slotButton in slotButtons)
            slotButton.Refresh(playerSkillController, emptySlotIcon);
    }
}