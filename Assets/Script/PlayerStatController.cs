using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatController : MonoBehaviour
{
    public BaseStatData baseStats;                    // 기본 스탯
    public PlayerTraitData playerTraitData;           // 플레이어 속성/격/패시브 정보
    public List<ActiveEffect> activeEffects = new();  // 현재 적용 중인 효과 목록
    private Dictionary<StatType, float> finalStats = new(); // 계산이 끝난 최종 스탯 저장

    public bool isDirty = true;                       // 재계산 필요 여부

    // TODO:
    // - 최종 계산된 스탯을 캐시할 딕셔너리 또는 구조 추가
    // - 예: Attack, Defense, MaxHp 등의 최종 결과 저장
  

    public void AddEffect(EffectData effectData, string sourceId)
    {
        // TODO:
        // 1. effectData와 sourceId로 ActiveEffect 생성
        ActiveEffect effect = new ActiveEffect(effectData, sourceId,effectData.duration,1,false,effectData.isPermanent);

        // 2. activeEffects 리스트에 추가
        activeEffects.Add(effect);
        // 3. isDirty = true 처리
        isDirty = true;
    }


    public void RemoveEffectsBySource(string sourceId)
    {
        // TODO:
        // 1. sourceId와 같은 효과들을 activeEffects에서 제거

        int removedCount = activeEffects.RemoveAll(effect => effect.sourceId == sourceId);

        if (removedCount > 0) // 2. isDirty = true 처리
        {
            isDirty = true; // 효과가 제거됐을 때만 재계산
        }

    }

    public void RemoveExpiredEffects()
    {
        // TODO:
       
       

        foreach (ActiveEffect effect in activeEffects.ToList<ActiveEffect>()) {

            

            // 1. 남은 시간이 0 이하인 효과 제거
            if (effect.isExpired) {
                activeEffects.Remove(effect);
                // 2. 제거되었으면 isDirty = true
                isDirty = true;
            }
        }
       
    }

    public void UpdateTimedEffects(float deltaTime)
    {
        // TODO:
        
        foreach (ActiveEffect effect in activeEffects) {
            

            if (effect.isPermanent) // 영구 효과는 시간 계산하지 않음
                continue;

            // 1. duration이 있는 ActiveEffect들의 remainingDuration 감소
            effect.remainingDuration -= deltaTime;
           
            // 2. 0 이하가 되면 isExpired 처리
            if(effect.remainingDuration <= 0)
            {
                effect.isExpired=true;
              
            }
        }

        RemoveExpiredEffects();
        // 3. 마지막에 RemoveExpiredEffects 호출 가능
    }

    public void RecalculateStats()
    {
        // TODO:
        // 1. baseStats 기준으로 시작
        // 2. activeEffects 중 PassiveAlways / 지속 버프 효과 적용
        // 3. Flat -> PercentAdd -> PercentMul 순서로 계산
        // 4. 결과 캐시에 저장
        // 5. isDirty = false
        float attack = baseStats.attack; // 기본 공격력

        float flatBonus = 0f;       // 고정 공격력 증가
        float percentAdd = 0f;      // 합산 배율 증가
        float percentMultiply = 1f; // 최종 곱연산 증가

        if (activeEffects != null)
        {
            foreach (ActiveEffect effect in activeEffects)
            {
                if (effect == null || effect.isExpired) // 만료 효과 제외
                    continue;

                EffectData effectData = effect.effectData;

                if (effectData == null) // 데이터 검사
                    continue;

                if (effectData.targetStat != StatType.Attack) // 공격력 효과만 계산
                    continue;

                switch (effectData.modifierType)
                {
                    case ModifierType.Flat:
                        flatBonus += effectData.value; // 공격력 +10
                        break;

                    case ModifierType.PercentAdd:
                        percentAdd += effectData.value; // 공격력 +50%
                        break;

                    case ModifierType.PercentMul:
                        percentMultiply *= effectData.value; // 최종 공격력 1.5배
                        break;

                        // Override와 Clamp는 나중에 추가
                }
            }
        }

        attack += flatBonus;              // 고정값 먼저 적용
        attack *= 1f + percentAdd;        // 합산 배율 적용
        attack *= percentMultiply;        // 최종 배율 적용

        finalStats[StatType.Attack] = attack; // 계산 결과 저장
        isDirty = false;                       // 재계산 완료
    }

    public float GetFinalStat(StatType statType)
    {

        // TODO:
        // 1. isDirty면 RecalculateStats 호출
        // 2. 캐시된 최종 스탯 반환

        if (isDirty) // 효과가 변경됐다면 다시 계산
        {
            RecalculateStats();
        }

        if (finalStats.TryGetValue(statType, out float finalValue))
        {
            return finalValue; // 저장된 최종 스탯 반환
        }

        return GetBaseStat(statType); // 아직 계산하지 않은 스탯
    }

    public bool HasPassive(string passiveId)
    {
        // TODO:
        // 1. playerTraitData.passiveIds 안에 passiveId 있는지 검사
        return false;
    }

    public bool HasElement(ElementType elementType)
    {
        // TODO:
        // 1. mainElement 또는 subElements에 해당 속성이 있는지 검사
        return false;
    }

    public int GetRankValue()
    {
        // TODO:
        // playerTraitData의 rankValue 반환
        return 0;
    }

    private void Update()
    {
        if (activeEffects == null || activeEffects.Count == 0)
            return;
        UpdateTimedEffects(Time.deltaTime);
    }

    private float GetBaseStat(StatType statType)
    {
        // 요청한 종류의 기본 스탯 반환
        return statType switch
        {
            StatType.Attack => baseStats.attack,
            StatType.Defense => baseStats.defense,
            StatType.MaxHp => baseStats.maxHp,
            StatType.MaxMp => baseStats.maxMp,
            StatType.MoveSpeed => baseStats.moveSpeed,
            StatType.KnockBack => baseStats.knockBack,
            StatType.CritRate => baseStats.critRate,
            StatType.CritDamage => baseStats.critDamage,
            StatType.CastSpeed => baseStats.castSpeed,
            StatType.CooldownRate => baseStats.cooldownRate,
            _ => 0f // 정의되지 않은 StatType만 0
        };
    }
}
