using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : MonoBehaviour
{
    public EquipmentData equippedWeapon;          // 현재 무기
    public EquipmentData equippedArmor;           // 현재 방어구
    public List<EquipmentData> equippedAccessories = new(); // 장신구 목록

    public PlayerStatController playerStatController;   // 플레이어 스탯 계산기
    public ElementResolver elementResolver;             // 속성 검사기
    public SkillValidationService skillValidationService; // 공용 검사기 사용 가능

    public bool Equip(EquipmentData equipmentData)
    {
        // TODO:
        // 1. CanEquip 검사
        RestrictionResult restrictionResult = CanEquip(equipmentData);

        // 2. 가능하면 해당 슬롯에 장비 장착
        // 3. ApplyEquipmentEffects 호출
        // 4. true 반환
        if (restrictionResult.isAllowed == true)
        {
            switch (equipmentData.slotType) {
                case EquipmentSlotType.Weapon: equippedWeapon = equipmentData; break;
                case EquipmentSlotType.Armor: equippedArmor = equipmentData; break;
                case EquipmentSlotType.Accessory: equippedAccessories.Add(equipmentData); break;
            }
            ApplyEquipmentEffects(equipmentData);
            return true;
        }
     
        return false;
    }

    public void Unequip(EquipmentSlotType slotType)
    {
        // TODO:
        EquipmentData equipmentData;
        // 1. slotType에 맞는 장비 찾기
        
        switch (slotType)
        {
            case EquipmentSlotType.Weapon:
                equipmentData = equippedWeapon;
                if (equipmentData != null)
                    RemoveEquipmentEffects(equipmentData);
                equippedWeapon = null;
                break;
            case EquipmentSlotType.Armor:
                equipmentData = equippedArmor;
                if (equipmentData != null)
                    RemoveEquipmentEffects(equipmentData);
                equippedArmor = null;
                break;
            case EquipmentSlotType.Accessory:
                equipmentData = equippedArmor;//test용
                //장신구별이라 따로 로직생각할것
                if (equipmentData != null)
                    RemoveEquipmentEffects(equipmentData);
                equippedArmor = null;
                break;
        
        }
        // 2. RemoveEquipmentEffects 호출
        
        // 3. 장비 슬롯 비우기
    }

    public RestrictionResult CanEquip(EquipmentData equipmentData)
    {
        RestrictionResult restrictionResult = new RestrictionResult();
        // TODO:
        // 1. 격 요구치 확인

        if (equipmentData.requiredRank > playerStatController.playerTraitData.rankValue)
        {
            restrictionResult.hasPenalty=true;
        }
        // 2. 속성 충돌 검사
        // 3. 패시브/플레이어 속성과 충돌하는지 검사
        // 4. Allow / Penalty / Block 결과 반환
        return new RestrictionResult();
    }

    public void ApplyEquipmentEffects(EquipmentData equipmentData)
    {
        // TODO:
        // 1. equipmentData.grantedEffects 순회
        foreach (EffectData effectData in equipmentData.grantedEffects)
        {
            playerStatController.AddEffect(effectData, equipmentData.id);
        }
        // 2. 플레이어 스탯 컨트롤러에 AddEffect
    }

    public void RemoveEquipmentEffects(EquipmentData equipmentData)
    {
        // TODO:
        // 1. equipmentData.id 기준으로 RemoveEffectsBySource 호출

        playerStatController.RemoveEffectsBySource(equipmentData.id);
    }

    public bool HasConflictingElement(EquipmentData equipmentData)
    {
        // TODO:
        // 1. 플레이어 속성과 장비 속성 충돌하는지 검사
        return false;
    }
}
