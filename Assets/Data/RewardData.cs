using UnityEngine;

public enum RewardType
{
    SkillUnlock // 스킬을 영구 해금하고 현재 회차에도 추가
}

[CreateAssetMenu(fileName = "NewRewardData", menuName = "Game/Reward Data")]
public class RewardData : ScriptableObject
{
    public RewardType rewardType = RewardType.SkillUnlock;
    public SkillData skillData;
    public int amount = 1;
}
