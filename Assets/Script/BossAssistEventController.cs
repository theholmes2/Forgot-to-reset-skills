using System.Collections;
using UnityEngine;

public class BossAssistEventController : MonoBehaviour
{
    public BossStageController bossStageController; // 보스전 관리자
    public PlayerHealth playerHealth; // 플레이어 체력 확인용

    public GameObject assistNpcObject; // 도와주러 오는 NPC
    public GameObject assistMessagePanel; // "누군가 포탈을 열었다" 같은 UI

    public float assistTime = 30f; // 30초 지나면 발동
    public float assistHealthRate = 0.3f; // 체력 30% 이하이면 발동
    public float messageShowTime = 2f; // 문구 표시 시간

    private bool isChecking; // 조건 확인 중인지
    private bool isTriggered; // 이미 발동됐는지
    private Coroutine checkRoutine;

    public Transform assistSpawnPoint;
    public SupportCharacterController assistCharacter;

    private void Awake()
    {
        if (bossStageController == null)
            bossStageController = GetComponentInParent<BossStageController>();

        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (assistNpcObject != null)
            assistNpcObject.SetActive(false); // 시작할 때 NPC 숨김

        if (assistMessagePanel != null)
            assistMessagePanel.SetActive(false); // 시작할 때 메시지 숨김

        if (assistCharacter == null && assistNpcObject != null)
            assistCharacter = assistNpcObject.GetComponent<SupportCharacterController>();
    }

    public void BeginAssistCheck()
    {
        if (isTriggered)
            return;

        if (checkRoutine != null)
            StopCoroutine(checkRoutine);

        checkRoutine = StartCoroutine(AssistCheckRoutine());
    }

    public void StopAssistCheck()
    {
        isChecking = false;

        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
        }
    }

    private IEnumerator AssistCheckRoutine()
    {
        isChecking = true;

        float timer = 0f;

        while (isChecking)
        {
            if (bossStageController == null)
                yield break;

            if (bossStageController.currentState != BossStageState.Battle)
                yield break; // 보스전 중이 아니면 중단

            timer += Time.deltaTime;

            bool isTimeOver = timer >= assistTime;
            bool isPlayerLowHealth = playerHealth != null && playerHealth.GetHealthRate() <= assistHealthRate;

            if (isTimeOver || isPlayerLowHealth)
            {
                TriggerAssist();
                yield break;
            }

            yield return null;
        }
    }

    private void TriggerAssist()
    {
        if (isTriggered)
            return;

        isTriggered = true;
        isChecking = false;

        if (assistNpcObject != null && assistSpawnPoint != null)
            assistNpcObject.transform.SetPositionAndRotation(assistSpawnPoint.position, assistSpawnPoint.rotation);

        if (assistNpcObject != null)
            assistNpcObject.SetActive(true);

        if (assistCharacter != null && bossStageController != null && bossStageController.bossObject != null)
            assistCharacter.BeginAssist(bossStageController.bossObject.transform);

        if (bossStageController != null)
            bossStageController.OpenEscapePortalByAssist();

        StartCoroutine(MessageRoutine());
    }

    private IEnumerator MessageRoutine()
    {
        if (assistMessagePanel != null)
            assistMessagePanel.SetActive(true);

        yield return new WaitForSeconds(messageShowTime);

        if (assistMessagePanel != null)
            assistMessagePanel.SetActive(false);
    }
}