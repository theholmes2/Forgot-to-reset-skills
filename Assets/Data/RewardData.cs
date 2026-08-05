using UnityEngine;

public enum RewardType
{
    SkillUnlock, // 스킬을 영구 해금하고 현재 회차에도 추가
    SkillPoint // 스킬트리 노드 해금에 사용하는 영구 포인트
}

[CreateAssetMenu(fileName = "NewRewardData", menuName = "Game/Reward Data")]
public class RewardData : ScriptableObject
{
    public string id; // 일회성 보상 식별자. 스킬 포인트 보상에는 반드시 입력
    public RewardType rewardType = RewardType.SkillUnlock;
    public SkillData skillData;
    public int amount = 1;
}
