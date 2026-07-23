using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillEquipItemUI : MonoBehaviour
{
    public Button button;
    public Image iconImage;
    public TMP_Text nameText;

    private SkillData skillData;
    private SkillEquipUIManager manager;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnClick);
    }

    public void Init(SkillData data, SkillEquipUIManager owner)
    {
        skillData = data;
        manager = owner;

        iconImage.sprite = data.icon;
        iconImage.enabled = data.icon != null;

        nameText.text = data.displayName;
    }

    private void OnClick()
    {
        manager.SelectSkill(skillData);
    }
}