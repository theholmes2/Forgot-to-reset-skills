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

    private EnemyHealth bossHealth;

    
    private void Start()
    {
        currentState = BossStageState.Ready;

        // 보스별 등장 연출 자동 찾기
        if (bossIntroDirector == null)
            bossIntroDirector = GetComponentInChildren<BossIntroDirector>(true);

        // IntroDirector에 보스가 연결되어 있으면 거기서 가져오기
        if (bossObject == null && bossIntroDirector != null)
            bossObject = bossIntroDirector.bossObject;

        // 그래도 없으면 자식 중 EnemyHealth가 붙은 오브젝트를 보스로 찾기
        if (bossObject == null)
        {
            EnemyHealth foundBoss = GetComponentInChildren<EnemyHealth>(true);

            if (foundBoss != null)
                bossObject = foundBoss.gameObject;
        }

        // 시작할 때 보스 숨김
        if (bossObject != null)
            bossObject.SetActive(false);

        // UI 초기화
        if (clearPanel != null)
            clearPanel.SetActive(false);

        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        // 플레이어 자동 찾기
        if (player == null)
            player = FindAnyObjectByType<Player>();

        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerControlLock == null && player != null)
            playerControlLock = player.GetComponent<PlayerControlLock>();

        // 카메라 자동 찾기
        if (cameraFollow == null && Camera.main != null)
            cameraFollow = Camera.main.GetComponent<CameraDeadZoneFollow>();

        if (cameraFocusController == null && Camera.main != null)
            cameraFocusController = Camera.main.GetComponent<CameraFocusController>();

        // 플레이어 사망 감지 등록
        if (playerHealth != null)
            playerHealth.OnDied += OnPlayerDied;

        // 스테이지 시작 즉시 보스전이면 바로 시작
        if (spawnConditionType == BossSpawnConditionType.OnStageStart)
            StartBossBattle();

        
    }

    public void StartBossBattle()
    {
        if (currentState != BossStageState.Ready)
            return; // 이미 시작했으면 중복 실행 방지

        if (bossObject == null)
            return; // 보스가 없으면 시작 불가

        StartCoroutine(BossAppearRoutine());
    }

    private IEnumerator BossAppearRoutine()
    {
        currentState = BossStageState.Appearing;

        bool shouldLockPlayer = bossIntroDirector != null && bossIntroDirector.lockPlayerDuringIntro;
        bool shouldLockCamera = bossIntroDirector != null && bossIntroDirector.lockCameraDuringIntro;

        if (shouldLockPlayer && playerControlLock != null)
            playerControlLock.Lock("BossIntro"); // 이 보스가 원할 때만 조작 잠금

        if (shouldLockCamera && cameraFollow != null)
            cameraFollow.isLocked = true; // 이 보스가 원할 때만 카메라 고정

        if (bossIntroDirector != null &&
    bossIntroDirector.moveCameraToBoss &&
    cameraFocusController != null)
        {
            yield return StartCoroutine(
                cameraFocusController.MoveToTarget(
                    bossIntroDirector.cameraFocusTarget,
                    bossIntroDirector.cameraMoveTime
                )
            ); // 카메라를 보스 쪽으로 이동
        }


        if (bossIntroDirector != null)
        {
            yield return StartCoroutine(bossIntroDirector.PlayIntro()); // 보스별 등장 연출 실행
        }
        else
        {
            bossObject.SetActive(true); // 연출이 없으면 그냥 등장

            yield return new WaitForSeconds(appearMessageTime);
           
        }

        bossHealth = bossObject.GetComponent<EnemyHealth>();

        if (bossHealth != null)
            bossHealth.OnDied += OnBossDied; // 보스 사망 감지 등록

        Enemy bossEnemy = bossObject.GetComponent<Enemy>();
        if (bossEnemy != null)
            bossEnemy.ChangeState(Enemy.State.Chase); // 보스전 시작 후 추적 시작

        if (shouldLockCamera && cameraFollow != null)
            cameraFollow.isLocked = false; // 잠근 카메라 다시 따라가게 함

       

        if (shouldLockPlayer && playerControlLock != null)
            playerControlLock.Unlock("BossIntro"); // 잠근 조작 해제

        currentState = BossStageState.Battle;
    }

    private void OnBossDied(EnemyHealth deadBoss)
    {
        if (currentState == BossStageState.Clear || currentState == BossStageState.Defeat)
            return;

        if (bossHealth != null)
            bossHealth.OnDied -= OnBossDied; // 이벤트 중복 방지

        ClearStage();
    }

    private void OnPlayerDied()
    {
        DefeatStage();
    }

    public void ClearStage()
    {
        currentState = BossStageState.Clear;

        Debug.Log("보스 클리어!");

        if (clearPanel != null)
            clearPanel.SetActive(true);

        if (cameraFollow != null)
            cameraFollow.isLocked = false; // 혹시 남은 카메라 락 해제

        if (playerControlLock != null)
            playerControlLock.Lock("StageEnd"); // 클리어 후 조작 막기

        // 나중에 보상 지급, 저장, 강제 회귀 연결
    }

    public void DefeatStage()
    {
        if (currentState == BossStageState.Clear || currentState == BossStageState.Defeat)
            return;

        currentState = BossStageState.Defeat;

        Debug.Log("플레이어 패배");

        if (defeatPanel != null)
            defeatPanel.SetActive(true);

        if (cameraFollow != null)
            cameraFollow.isLocked = false; // 혹시 남은 카메라 락 해제

        if (playerControlLock != null)
            playerControlLock.Lock("StageEnd"); // 패배 후 조작 막기

        // 나중에 GameManager.Instance.OnPlayerDeath() 강제 호출
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
            bossHealth.OnDied -= OnBossDied;

        if (playerHealth != null)
            playerHealth.OnDied -= OnPlayerDied;
    }
}