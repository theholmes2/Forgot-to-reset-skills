using UnityEngine;
using System;

public enum SkillSpawnType
{
    PlayerPosition,   // 플레이어 위치에서 생성
    AttackPoint,      // 플레이어 앞 AttackPoint에서 생성
    ForwardOffset,    // 플레이어 앞 일정 거리에서 생성
    TargetPosition    // 타겟 위치에 생성
}

public enum SkillSlotKey
{
    J,
    L,
    U,
    I,
    O
}

public enum SkillAnimationType
{
    None,
    Attack,
    Magic,
    Dash,
    Buff
}

public class PlayerSkillController : MonoBehaviour
{
    public SkillSlot jSlot;
    public SkillSlot lSlot;
    public SkillSlot uSlot;
    public SkillSlot iSlot;
    public SkillSlot oSlot;

    public event Action OnSlotChanged;

    public Transform attackPoint; //공격 위치 기준

    public SPUM_Prefabs PrefabsController; //애니메이션 참조

    private void Update()
    {
        UpdateCooldown(jSlot);
        UpdateCooldown(lSlot);
        UpdateCooldown(uSlot);
        UpdateCooldown(iSlot);
        UpdateCooldown(oSlot);
    }

    private void UpdateCooldown(SkillSlot slot) //쿨타임 변경
    {
        if (slot == null || !slot.isCooldown) return; //슬롯이 비었거나 + 쿨타임이 아니면 리턴

        slot.remainCooldown -= Time.deltaTime; //실제 쿨타임 줄이기

        if (slot.remainCooldown <= 0f) //쿨타임 0초면
        {
            slot.remainCooldown = 0f;
            slot.isCooldown = false; //쿨타임 아니다
        }
    }

    public SkillSlot GetSlot(SkillSlotKey key) //받은 키로 슬롯 리턴
    {
        switch (key)
        {
            case SkillSlotKey.J: return jSlot;
            case SkillSlotKey.L: return lSlot;
            case SkillSlotKey.U: return uSlot;
            case SkillSlotKey.I: return iSlot;
            case SkillSlotKey.O: return oSlot;
        }

        return null;
    }

    public void NotifySlotChanged() //슬롯 변경시 이벤트 알림
    {
        OnSlotChanged?.Invoke();
    }

    public void SetSkill(SkillSlotKey key, SkillData data) //키와 데이터 받아서 스킬 세팅
    {
        SkillSlot slot = GetSlot(key); //받은 키로 슬롯 고름
        if (slot == null) return;

        slot.skillData = data; //골라둔 슬롯에 스킬데이터 넣음
        slot.isCooldown = false; //쿨 초기화
        slot.remainCooldown = 0f;

        NotifySlotChanged(); //변경했다 알림
    }

    public bool UseJSkill()
    {
        return UseSlotSkill(jSlot);
    }

    public bool UseUSkill()
    {
        return UseSlotSkill(uSlot);
    }

    public bool UseISkill()
    {
        return UseSlotSkill(iSlot);
    }

    public bool UseOSkill()
    {
        return UseSlotSkill(oSlot);
    }

    public bool UseLSkill()
    {
        return UseSlotSkill(lSlot);
    }

    public bool UseSlotSkill(SkillSlot slot) //플레이어가 키를 누르면 실행됨
    {
        if (slot == null)
            return false;

        if (slot.skillData == null)
            return false;

        if (slot.isCooldown)
            return false;

        bool isUsed = UseSkill(slot.skillData); //슬롯 스킬 사용

        if (isUsed)
        {
            PlaySkillAnimation(slot.skillData.animationType); //애니메이션 재생요청

            slot.isCooldown = true; //쿨타임 시작
            slot.remainCooldown = slot.skillData.cooldown; //현재 쿨타임 초기화

            return true;
        }

        return false;
    }

    void PlaySkillAnimation(SkillAnimationType skillanim) //타입별 애니 재생
    {
        if (PrefabsController == null)
            return;

        switch (skillanim)
        {
            case SkillAnimationType.Attack:
                PrefabsController.AttackAnimation();
                break;
        }
    }

    private bool UseSkill(SkillData skillData)
    {
        if (skillData == null) //스킬검사
            return false;

        if (skillData.skillPrefab == null) //실제 스킬 범위 및 이펙트
            return false;

        Vector3 spawnPosition = transform.position; //플레이어 위치로 포지션 설정

        if (skillData.spawnType == SkillSpawnType.AttackPoint && attackPoint != null) //만약 스킬이 어택포인트에서 생성된다면 + 포인트검사
        {
            spawnPosition = attackPoint.position; //어택포인트로 위치 변경
        }

        GameObject skillObject = Instantiate(skillData.skillPrefab, spawnPosition, transform.rotation); //프리팹을 설정된 위치에 생성
        skillObject.transform.parent = transform;

        BasicAttackHitBox hitBox = skillObject.GetComponent<BasicAttackHitBox>();

        if (hitBox != null)
        {
            hitBox.Init(skillData.damage);
        }

        return true;
    }
}
