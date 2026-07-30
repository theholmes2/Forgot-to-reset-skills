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
    public CombatResolver combatResolver; // 최종 데미지 계산 담당

    public PlayerAnimationController animationController;
    public PlayerStatController playerStatController;
    private void Awake()
    {
        if (combatResolver == null)
            combatResolver = GetComponent<CombatResolver>();

        if (animationController == null)
            animationController = GetComponent<PlayerAnimationController>();

        if (playerStatController == null)
            playerStatController = GetComponent<PlayerStatController>();

        if (attackPoint == null)
        {
            Transform facingRoot = transform.Find("FacingRoot");

            if (facingRoot != null)
                attackPoint = facingRoot.Find("AttackPoint");
        }
    }

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

    private void PlaySkillAnimation(SkillAnimationType skillAnimation)
    {
        if (animationController == null)
            return;

        switch (skillAnimation)
        {
            case SkillAnimationType.Attack:
                animationController.PlayAttack();
                break;
        }
    }

    private bool UseSkill(SkillData skillData)
    {
        if (skillData == null) //스킬검사
            return false;

        if (skillData.isBuffSkill) //버프스킬이라면 (일단 프리팹 없어서 위에둠)
        {
            foreach (EffectData effect in skillData.effects)
            {
                playerStatController.AddEffect(effect, skillData.id);
            }
            return true;
        }

        if (skillData.skillPrefab == null) //실제 스킬 범위 및 이펙트
            return false;

        if (combatResolver == null) // 데미지 계산기 검사
            return false;

        Vector3 spawnPosition = transform.position; //플레이어 위치로 포지션 설정

        if (skillData.spawnType == SkillSpawnType.AttackPoint && attackPoint != null) //만약 스킬이 어택포인트에서 생성된다면 + 포인트검사
        {
            spawnPosition = attackPoint.position; //어택포인트로 위치 변경
        }

        Quaternion spawnRotation = skillData.skillPrefab.transform.rotation;

        GameObject skillObject = Instantiate(
            skillData.skillPrefab,
            spawnPosition,
            spawnRotation
        );


        BasicAttackHitBox hitBox = skillObject.GetComponent<BasicAttackHitBox>();


        if (hitBox != null)
        {
            skillObject.transform.parent = transform; // 근접 히트박스만 플레이어 자식으로 붙임
            float finalDamage = combatResolver.GetFinalAttackDamage(skillData); // 최종 데미지 계산

            hitBox.Init(finalDamage, transform); // 계산된 데미지를 공격 판정에 전달,내위치도 전달 
        }


        ProjectileSkillHitBox projectile = skillObject.GetComponent<ProjectileSkillHitBox>();
        if (projectile != null)
        {
            float finalDamage = combatResolver.GetFinalAttackDamage(skillData); // 최종 데미지 계산

            projectile.Init(finalDamage, transform, skillData.id); // 투사체에 데미지/주인/스킬id 전달
        }

        return true;
    }

    public void SetSkill(SkillSlotKey key, SkillData data) //키와 데이터 받아서 스킬 세팅
    {
        SkillSlot targetSlot = GetSlot(key); //받은 키로 슬롯 고름
        if (targetSlot == null) return;

        if (data != null)
        {
            ClearSameSkillFromOtherSlots(key, data); // 같은 스킬이 다른 슬롯에 있으면 제거
        }

        targetSlot.skillData = data; //비어있는 슬롯에도 스킬 장착 가능
        targetSlot.isCooldown = false; //쿨 초기화
        targetSlot.remainCooldown = 0f;

        NotifySlotChanged(); //변경했다 알림
    }

    private void ClearSameSkillFromOtherSlots(SkillSlotKey targetKey, SkillData data)
    {
        ClearSameSkillFromSlot(SkillSlotKey.J, targetKey, data);
        ClearSameSkillFromSlot(SkillSlotKey.L, targetKey, data);
        ClearSameSkillFromSlot(SkillSlotKey.U, targetKey, data);
        ClearSameSkillFromSlot(SkillSlotKey.I, targetKey, data);
        ClearSameSkillFromSlot(SkillSlotKey.O, targetKey, data);
    }

    private void ClearSameSkillFromSlot(SkillSlotKey checkKey, SkillSlotKey targetKey, SkillData data)
    {
        if (checkKey == targetKey)
            return; // 지금 장착할 슬롯은 비우면 안 됨

        SkillSlot slot = GetSlot(checkKey);
        if (slot == null || slot.skillData == null)
            return;

        if (slot.skillData == data || slot.skillData.id == data.id)
        {
            slot.skillData = null; // 같은 스킬이면 기존 슬롯에서 제거
            slot.isCooldown = false;
            slot.remainCooldown = 0f;
        }
    }

}
