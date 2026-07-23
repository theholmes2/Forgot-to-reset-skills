using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillEquipUIManager : MonoBehaviour
{
    [System.Serializable]
    public class EquipSlotUI
    {
        public SkillSlotKey key;
        public Button button;
        public Image iconImage;
        public TMP_Text keyText;

        public void Refresh(PlayerSkillController controller)
        {
            SkillSlot slot = controller.GetSlot(key);

            if (keyText != null)
                keyText.text = key.ToString();

            if (slot == null || slot.skillData == null)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
                return;
            }

            iconImage.sprite = slot.skillData.icon;
            iconImage.enabled = slot.skillData.icon != null;
        }
    }

    public GameObject equipPanel;

    public PlayerSkillController playerSkillController;

    public Transform skillListRoot;
    public SkillEquipItemUI skillItemPrefab;

    public List<SkillData> allSkillDatas;
    public List<EquipSlotUI> slotUis;

    public TMP_Text selectedSkillNameText;
    public TMP_Text selectedSkillDescriptionText;

    private SkillData selectedSkill;

    private void Awake()
    {
        if (playerSkillController == null)
            playerSkillController = FindAnyObjectByType<PlayerSkillController>();

        foreach (EquipSlotUI slotUi in slotUis)
        {
            SkillSlotKey key = slotUi.key;

            slotUi.button.onClick.AddListener(() =>
            {
                EquipSelectedSkill(key);
            });
        }
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Open()
    {
        equipPanel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        equipPanel.SetActive(false);
    }

    public void Toggle()
    {
        equipPanel.SetActive(!equipPanel.activeSelf);

        if (equipPanel.activeSelf)
            Refresh();
    }

    public void SelectSkill(SkillData skillData)
    {
        selectedSkill = skillData;

        selectedSkillNameText.text = skillData.displayName;
        selectedSkillDescriptionText.text = skillData.description;
    }

    private void EquipSelectedSkill(SkillSlotKey key)
    {
        if (selectedSkill == null)
            return;

        playerSkillController.SetSkill(key, selectedSkill);

        RefreshSlots();
    }

    private void Refresh()
    {
        BuildSkillList();
        RefreshSlots();
    }

    private void BuildSkillList()
    {
        foreach (Transform child in skillListRoot)
            Destroy(child.gameObject);

        List<string> availableSkillPool = GameManager.Instance.runState.availableSkillPool;

        foreach (SkillData skillData in allSkillDatas)
        {
            if (skillData == null)
                continue;

            if (!availableSkillPool.Contains(skillData.id))
                continue;

            SkillEquipItemUI item = Instantiate(skillItemPrefab, skillListRoot);
            item.Init(skillData, this);
        }
    }

    private void RefreshSlots()
    {
        foreach (EquipSlotUI slotUi in slotUis)
            slotUi.Refresh(playerSkillController);
    }
}