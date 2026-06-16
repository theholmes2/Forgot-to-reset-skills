using System.Collections;
using UnityEngine;
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
    

    public Transform attackPoint; //공격 위치 기준

    public SPUM_Prefabs PrefabsController; //애니메이션 참조


  
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
    public bool UseSlotSkill(SkillSlot slot) //플레이어가 J키를 누르면 실행됨
    {
        if(slot == null) 
            return false;
        if (slot.isCooldown)
            return false;

        bool isUsed = UseSkill(slot.skillData);  //J 슬롯 스킬을 사용

        if (isUsed)
        {
            // J 쿨타임 코루틴 시작
            PlaySkillAnimation(slot.skillData.animationType); //애니메이션 재생요청
            StartCoroutine(SkillCooldown(slot, slot.skillData.cooldown));
            return true;
        }
        return false;
    }
    void PlaySkillAnimation(SkillAnimationType skillanim)  //타입별 애니 재생
    {
        if (PrefabsController == null)
            return;

        switch (skillanim) {
            case SkillAnimationType.Attack:
                PrefabsController.AttackAnimation(); 
                break;
        }

    }

    IEnumerator SkillCooldown(SkillSlot skill, float cooldown)
    {
        skill.isCooldown = true;
        yield return new WaitForSeconds(cooldown);
        skill.isCooldown = false;

    }
    private bool UseSkill(SkillData skillData)
    {
        if (skillData == null) //스킬검사 
            return false;
        if (skillData.skillPrefab == null) // 실제 스킬 범위 및 이펙트 
            return false;

        Vector3 spawnPosition = transform.position; //플레이어 위치로 포지션 설정

        if (skillData.spawnType == SkillSpawnType.AttackPoint && attackPoint != null) //만약 스킬이 어택포인트에서 생성된다면 + 포인트검사
        {
            spawnPosition = attackPoint.position; //어택포인트로 위치 변경
        }

        GameObject skillObject = Instantiate(skillData.skillPrefab, spawnPosition, transform.rotation); //프리팹을 설정된 위치에 생성
        BasicAttackHitBox hitBox = skillObject.GetComponent<BasicAttackHitBox>();

        if (hitBox != null)
        {
            hitBox.Init(skillData.damage);
        } 

        return true;

    }

   
}
