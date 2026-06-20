using UnityEngine;

public class SkillUIManager : MonoBehaviour
{
    public PlayerSkillController PlayerSkillController; //플레이어 스킬 컨트롤러

    public SkillUIBinding[] skillBindings; //J/L/U/I/O 슬롯들은 배열로 한 번에 관리

    public SkillSlotUI KSlotUI; //점프 전용 슬롯, 일반 스킬 슬롯과 분리해서 관리

    private void Awake()
    {
        //스킬 컨트롤러 찾기
        if (PlayerSkillController == null)
            PlayerSkillController = FindAnyObjectByType<PlayerSkillController>();

        //점프 전용 K 슬롯 초기화
        RefreshJumpSlotUI();
    }

    private void OnEnable() //활성화 시
    {
        if (PlayerSkillController != null)
            PlayerSkillController.OnSlotChanged += RefreshAllSlotsUI; //슬롯 변경 이벤트 등록
    }

    private void OnDisable() //비활성화 시
    {
        if (PlayerSkillController != null)
            PlayerSkillController.OnSlotChanged -= RefreshAllSlotsUI; //슬롯 변경 이벤트 제거
    }

    private void Start()
    {
        RefreshAllSlotsUI(); //최초 UI 설정
    }

    private void Update()
    {
        if (PlayerSkillController == null) return; //컨트롤러 없으면 종료

        foreach (var binding in skillBindings) //전체 슬롯 순회
        {
            SkillSlot slot = PlayerSkillController.GetSlot(binding.slotKey); //키에 맞는 슬롯 가져오기
            UpdateCooldownUI(slot, binding.slotUI); //쿨타임 UI만 갱신
        }
    }

    private bool HasAnyActiveCooldown()
    {
        foreach (var binding in skillBindings) //전체 바인딩 확인
        {
            SkillSlot slot = PlayerSkillController.GetSlot(binding.slotKey); //키에 맞는 슬롯 가져오기

            if (slot != null && slot.isCooldown) //하나라도 쿨타임이면 true
                return true;
        }

        return false; //전부 쿨타임 아니면 false
    }

    private void RefreshAllSlotsUI() //전체 슬롯 UI 재설정
    {
        if (PlayerSkillController == null) return;

        foreach (var binding in skillBindings) //J/L/U/I/O 전체 확인
        {
            SkillSlot slot = PlayerSkillController.GetSlot(binding.slotKey); //키에 맞는 슬롯 가져오기
            RefreshSlotUI(slot, binding.slotUI, binding.slotKey.ToString()); //슬롯 UI 갱신
        }

        RefreshJumpSlotUI(); //점프 전용 K 슬롯도 따로 갱신
    }

    private void RefreshSlotUI(SkillSlot slot, SkillSlotUI slotUI, string keyName) //슬롯 UI 재설정
    {
        if (slotUI == null) return;

        slotUI.keyText.text = keyName; //무슨 키인지 텍스트 설정

        if (slot == null || slot.skillData == null) //슬롯 없거나 스킬 데이터 없으면
        {
            slotUI.iconImage.sprite = null;
            slotUI.iconImage.enabled = false; //아이콘 끄기
            slotUI.cooldownCoverImage.gameObject.SetActive(false); //쿨타임 커버 끄기
            return;
        }

        //슬롯과 스킬 데이터가 있으면
        slotUI.iconImage.enabled = true;
        slotUI.iconImage.sprite = slot.skillData.icon; //아이콘 교체

        UpdateCooldownUI(slot, slotUI); //현재 쿨타임 상태 반영
    }

    private void RefreshJumpSlotUI() //점프 전용 K 슬롯 UI 설정
    {
        if (KSlotUI == null) return;

        KSlotUI.keyText.text = "K"; //K키 고정
        KSlotUI.cooldownCoverImage.gameObject.SetActive(false); //일단 쿨타임 커버 끔

        //아직 점프 스킬 데이터 구조가 없으니 아이콘은 비워둠
        KSlotUI.iconImage.sprite = null;
        KSlotUI.iconImage.enabled = false;

        //나중에 엔젤 점프 / 플라잉 / 악마의 날개짓 같은 점프 전용 스킬 붙일 때
        //여기서 K 전용 데이터 받아서 별도 갱신하면 됨
    }

    private void UpdateCooldownUI(SkillSlot slot, SkillSlotUI slotUI)
    {
        if (slotUI == null) return;

        if (slot == null || slot.skillData == null) //슬롯이 없거나 스킬 데이터 없으면
        {
            slotUI.cooldownCoverImage.gameObject.SetActive(false); //쿨타임 커버 끄기
            return;
        }

        if (!slot.isCooldown || slot.remainCooldown <= 0f) //쿨타임 아니거나 0 이하이면
        {
            slotUI.cooldownCoverImage.gameObject.SetActive(false); //커버 끄기
            slotUI.cooldownCoverImage.fillAmount = 0f; //비율 0
            return;
        }

        //쿨타임 중이면
        slotUI.cooldownCoverImage.gameObject.SetActive(true); //커버 켜기

        float maxCooldown = slot.skillData.cooldown; //최대 쿨타임

        if (maxCooldown <= 0f) //쿨타임 값 이상하면 방어코드
        {
            slotUI.cooldownCoverImage.fillAmount = 0f;
            return;
        }

        float normalized = slot.remainCooldown / maxCooldown; //현재 쿨 비율
        slotUI.cooldownCoverImage.fillAmount = normalized; //커버 비율 반영
    }
}
