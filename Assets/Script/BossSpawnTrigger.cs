using UnityEngine;

public class BossSpawnTrigger : MonoBehaviour
{
    public BossStageController bossStageController; // 보스전 컨트롤러

    private bool isUsed;
    private void Awake()
    {
        if (bossStageController == null)
            bossStageController = GetComponentInParent<BossStageController>(); // 부모에서 자동 찾기
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isUsed)
            return;

        if (!collision.CompareTag("Player"))
            return;

        isUsed = true;

        if (bossStageController != null)
            bossStageController.StartBossBattle(); // 보스전 시작
    }
}