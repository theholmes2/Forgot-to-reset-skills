using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossRewardController : MonoBehaviour
{
    [Header("Reward")]
    public List<RewardData> rewards = new(); // 보스 처치 시 지급할 보상 목록

    [Header("UI")]
    public GameObject rewardPanel; // 보상 획득 안내 패널
    public TMP_Text rewardText; // 획득한 보상 이름을 표시할 텍스트
    public float rewardPanelShowTime = 2f;

    private SkillTreeController skillTreeController;
    private bool hasGivenRewards; // 같은 클리어에서 중복 지급 방지
    private Coroutine rewardPanelRoutine;

    private void Awake()
    {
        skillTreeController = FindAnyObjectByType<SkillTreeController>();

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    public bool GiveRewards()
    {
        if (hasGivenRewards)
            return false; // 이미 지급했다면 다시 처리하지 않음

        hasGivenRewards = true;

        if (skillTreeController == null)
            skillTreeController = FindAnyObjectByType<SkillTreeController>();

        if (skillTreeController == null)
        {
            Debug.LogWarning("보상 지급 실패: SkillTreeController를 찾을 수 없습니다.");
            return false;
        }

        bool hasChanged = false;
        List<string> receivedRewardNames = new();

        foreach (RewardData reward in rewards)
        {
            if (reward == null)
                continue;

            switch (reward.rewardType)
            {
                case RewardType.SkillUnlock:
                    if (reward.skillData == null)
                        continue;

                    bool isNewReward = skillTreeController.GrantPermanentSkillReward(reward.skillData);

                    if (!isNewReward)
                        continue; // 기존 보상이면 데이터만 보정하고 획득 UI는 표시하지 않음

                    hasChanged = true;
                    receivedRewardNames.Add(reward.skillData.displayName);
                    break;

                case RewardType.SkillPoint:
                    if (string.IsNullOrEmpty(reward.id))
                    {
                        Debug.LogWarning("스킬 포인트 보상 ID가 비어 있습니다.");
                        continue;
                    }

                    if (!skillTreeController.GrantSkillPointReward(reward.id, reward.amount))
                        continue;

                    hasChanged = true;
                    receivedRewardNames.Add("스킬 포인트 " + reward.amount);
                    break;
            }
        }


        if (receivedRewardNames.Count > 0)
        {
            ShowRewardPanel(receivedRewardNames);

            if (GameSoundController.Instance != null)
                GameSoundController.Instance.PlayReward();
        }

        return hasChanged;
    }

    private void ShowRewardPanel(List<string> rewardNames)
    {
        if (rewardText != null)
            rewardText.text = string.Join("\n", rewardNames) + " 획득!";

        if (rewardPanel == null)
            return;

        if (rewardPanelRoutine != null)
            StopCoroutine(rewardPanelRoutine);

        rewardPanelRoutine = StartCoroutine(RewardPanelRoutine());
    }

    private IEnumerator RewardPanelRoutine()
    {
        rewardPanel.SetActive(true);

        yield return new WaitForSeconds(rewardPanelShowTime);

        rewardPanel.SetActive(false);
        rewardPanelRoutine = null;
    }
}
