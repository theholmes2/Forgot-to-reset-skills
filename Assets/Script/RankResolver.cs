using UnityEngine;

public class RankResolver : MonoBehaviour
{
    public bool CanAffectByRank(int userRank, int targetRank, EffectData effectData)
    {
        // TODO:
        // 1. effectData.ignoreRank 확인
        // 2. 격 차이 계산
        // 3. 적용 가능 여부 판단
        return false;
    }

    public float GetRankMultiplier(int userRank, int targetRank, EffectData effectData)
    {
        // TODO:
        // 1. 격 차이에 따라 1.0 / 0.5 / 0 / 그 외 패널티 배율 반환
        // 2. ignoreRank면 1 반환 가능
        return 1f;
    }

    public bool IsRankIgnored(EffectData effectData)
    {
        // TODO:
        // effectData.ignoreRank 반환
        return false;
    }

    public float ApplyRankPenalty(float value, int userRank, int targetRank, EffectData effectData)
    {
        // TODO:
        // 1. GetRankMultiplier 호출
        // 2. value * rankMultiplier 반환
        return value;
    }
}
