using System.Collections;
using UnityEngine;

public enum BossSpawnConditionType
{
    OnStageStart,     // 스테이지 시작 즉시 등장
    OnPlayerEnter,    // 특정 구역 진입 시 등장
    OnQuestAccepted,  // 퀘스트 수락 시 등장
    Manual            // 다른 스크립트가 직접 호출
}

public enum BossStageState
{
    Ready,      // 보스전 시작 전
    Appearing,  // 등장 연출 중
    Battle,     // 전투 중
    Clear,      // 클리어
    Defeat      // 패배
}

public class BossStageController : MonoBehaviour
{
    public BossSpawnConditionType spawnConditionType = BossSpawnConditionType.OnPlayerEnter;
    public BossStageState currentState = BossStageState.Ready;

    [Header("Boss")]
    public GameObject bossObject; // 실제 보스 오브젝트
    public BossIntroDirector bossIntroDirector; // 보스별 등장 연출

    [Header("UI")]
    public GameObject clearPanel; // 클리어 UI
    public GameObject defeatPanel; // 패배 UI

    [Header("Player")]
    public Player player; // 플레이어
    public PlayerHealth playerHealth; // 플레이어 체력
    public PlayerControlLock playerControlLock; // 플레이어 조작 잠금

    [Header("Camera")]
    public CameraDeadZoneFollow cameraFollow; // 카메라 따라가기/고정
    public CameraFocusController cameraFocusController; // 카메라 특정 위치 이동

    [Header("Fallback")]
    public float appearMessageTime = 1.5f; // 연출 스크립트가 없을 때 대기 시간

    [Header("Restriction Wall")]
    public RestrictionWall[] restrictionWalls; // 보스전 중에만 켤 제한 벽들

    private EnemyHealth bossHealth;
    private SkillTreeController skillTreeController;

    private BossSummonController bossSummonController; // 보스가 가진 잡몹 소환 컨트롤러

    private void Start()
    {
        currentState = BossStageState.Ready;

        if (bossIntroDirector == null)
            bossIntroDirector = GetComponentInChildren<BossIntroDirector>(true); // 보스 연출 자동 찾기

        if (bossObject == null && bossIntroDirector != null)
            bossObject = bossIntroDirector.bossObject; // 연출 쪽 보스 사용

        if (bossObject == null)
        {
            EnemyHealth foundBoss = GetComponentInChildren<EnemyHealth>(true);

            if (foundBoss != null)
                bossObject = foundBoss.gameObject; // 자식에서 보스 자동 찾기
        }

        if (bossObject != null)
            bossObject.SetActive(false); // 시작할 때 보스 숨김

        if (clearPanel != null)
            clearPanel.SetActive(false);

        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        if (player == null)
            player = FindAnyObjectByType<Player>();

        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerControlLock == null && player != null)
            playerControlLock = player.GetComponent<PlayerControlLock>();

        if (cameraFollow == null && Camera.main != null)
            cameraFollow = Camera.main.GetComponent<CameraDeadZoneFollow>();

        if (cameraFocusController == null && Camera.main != null)
            cameraFocusController = Camera.main.GetComponent<CameraFocusController>();

        if (playerHealth != null)
            playerHealth.OnDied += OnPlayerDied; // 플레이어 사망 감지

        if (skillTreeController == null)
            skillTreeController = FindAnyObjectByType<SkillTreeController>();

        if (spawnConditionType == BossSpawnConditionType.OnStageStart)
            StartBossBattle(); // 스테이지 시작 즉시 보스전
    }

    public void StartBossBattle()
    {
        if (currentState != BossStageState.Ready)
            return; // 중복 시작 방지

        if (bossObject == null)
            return; // 보스 없으면 시작 불가

        StartCoroutine(BossAppearRoutine());
    }

    private IEnumerator BossAppearRoutine()
    {
        
        currentState = BossStageState.Appearing;
        SetRestrictionWalls(true); // 보스전 시작하면서 제한 벽 켜기
       
        

        bool shouldLockPlayer = bossIntroDirector != null && bossIntroDirector.lockPlayerDuringIntro;
        bool shouldLockCamera = bossIntroDirector != null && bossIntroDirector.lockCameraDuringIntro;

        if (shouldLockPlayer && playerControlLock != null)
            playerControlLock.Lock("BossIntro"); // 등장 중 조작 잠금

        if (shouldLockCamera && cameraFollow != null)
            cameraFollow.isLocked = true; // 등장 중 일반 카메라 추적 정지

        if (bossIntroDirector != null &&
            bossIntroDirector.moveCameraToBoss &&
            cameraFocusController != null)
        {
            yield return StartCoroutine(
                cameraFocusController.MoveToTarget(
                    bossIntroDirector.cameraFocusTarget,
                    bossIntroDirector.cameraMoveTime
                )
            ); // 먼저 보스 위치로 카메라 이동
        }

        if (bossIntroDirector != null)
        {
            Coroutine introRoutine = StartCoroutine(bossIntroDirector.PlayIntro()); // 글자 + 보스 등장 + 셰이크 시작

            if (bossIntroDirector.followBossDuringIntro &&
                cameraFocusController != null &&
                bossIntroDirector.cameraFocusTarget != null)
            {
                yield return StartCoroutine(
                    cameraFocusController.FollowTargetForSeconds(
                        bossIntroDirector.cameraFocusTarget,
                        bossIntroDirector.followBossTime,
                        bossIntroDirector.followBossSpeed
                    )
                ); // 셰이크 중에도 보스를 따라감
            }

            yield return introRoutine; // 연출이 아직 남아있으면 끝까지 기다림
        }
        else
        {
            bossObject.SetActive(true); // 연출이 없으면 그냥 등장
            yield return new WaitForSeconds(appearMessageTime);
        }

        bossHealth = bossObject.GetComponent<EnemyHealth>();
        bossSummonController = bossObject.GetComponent<BossSummonController>(); // 보스 소환 컨트롤러 찾기

        if (bossHealth != null)
            bossHealth.OnDied += OnBossDied; // 보스 사망 감지 등록

        Enemy bossEnemy = bossObject.GetComponent<Enemy>();

        if (bossEnemy != null)
            bossEnemy.ChangeState(Enemy.State.Chase); // 보스전 시작 후 추적 시작

        if (shouldLockCamera && cameraFollow != null)
            cameraFollow.isLocked = false; // 카메라 다시 플레이어 추적

        if (shouldLockPlayer && playerControlLock != null)
            playerControlLock.Unlock("BossIntro"); // 조작 잠금 해제

        if (bossSummonController != null)
            bossSummonController.StartSummon(); // 보스전 시작 후 잡몹 소환 시작
        currentState = BossStageState.Battle;
    }

    private void OnBossDied(EnemyHealth deadBoss)
    {
        if (currentState == BossStageState.Clear || currentState == BossStageState.Defeat)
            return;

        if (bossHealth != null)
            bossHealth.OnDied -= OnBossDied;

        ClearStage();
    }

    private void OnPlayerDied()
    {
        DefeatStage();
    }

    public void ClearStage()
    {
        currentState = BossStageState.Clear;
        SetRestrictionWalls(false); // 보스전 끝나면 제한 벽 끄기
        Debug.Log("보스 클리어!");

        if (clearPanel != null)
            clearPanel.SetActive(true);

        if (cameraFollow != null)
            cameraFollow.isLocked = false;

        if (playerControlLock != null)
            playerControlLock.Lock("StageEnd");

        if (bossSummonController != null)
            bossSummonController.StopSummon(true); // 클리어 시 잡몹 전부 정리

        // 나중에 보상 지급, 저장, 다음 스테이지 이동 추가
    }

    public void DefeatStage()
    {
        if (currentState == BossStageState.Clear || currentState == BossStageState.Defeat)
            return;

        currentState = BossStageState.Defeat;
        SetRestrictionWalls(false); // 보스전 끝나면 제한 벽 끄기
        Debug.Log("플레이어 패배");

        if (defeatPanel != null)
            defeatPanel.SetActive(true);

        if (cameraFollow != null)
            cameraFollow.isLocked = false;

        if (playerControlLock != null)
            playerControlLock.Lock("StageEnd");

        if (bossSummonController != null)
            bossSummonController.StopSummon(true); // 패배 시 잡몹 전부 정리
        // 나중에 GameManager.Instance.OnPlayerDeath() 강제 호출
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
            bossHealth.OnDied -= OnBossDied;

        if (playerHealth != null)
            playerHealth.OnDied -= OnPlayerDied;
    }
    private void SetRestrictionWalls(bool isActive)
    {
        if (restrictionWalls == null)
            return;

        foreach (RestrictionWall wall in restrictionWalls)
        {
            if (wall == null)
                continue;

            wall.SetWallActive(isActive); // 각 제한 벽 켜기/끄기
        }
    }

}