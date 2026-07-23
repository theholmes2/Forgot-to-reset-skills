using System;
using UnityEngine;

[Serializable]
public class BossSummonPhase
{
    [Range(0f, 1f)]
    public float healthPercent = 1f; // 이 체력 비율 이하일 때 적용

    public int maxAliveCount = 2; // 해당 구간의 최대 잡몹 수
    public float spawnInterval = 5f; // 해당 구간의 소환 간격
}

public class BossSummonController : MonoBehaviour
{
    public EnemyHealth bossHealth; // 보스 체력
    public EnemySpawner enemySpawner; // 보스가 사용할 잡몹 스포너

    [Header("Summon")]
    public bool startSummonOnEnable = false; // 켜질 때 바로 소환 시작할지
    public BossSummonPhase[] summonPhases; // 체력 비율별 소환 규칙

    private bool isSummoning;
    private BossSummonPhase currentPhase;

    private void Awake()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<EnemyHealth>(); // 보스 체력 자동 연결
    }

    private void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnDied += OnBossDied; // [추가] 보스 사망 시 잡몹 정리

        if (startSummonOnEnable)
            StartSummon(); // 테스트용 자동 시작
    }

    private void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnDied -= OnBossDied; // 이벤트 해제
    }

    private void Update()
    {
        if (!isSummoning)
            return;

        UpdateSummonPhase(); // 보스 체력에 맞춰 소환 규칙 갱신
    }

    public void StartSummon()
    {
        if (isSummoning)
            return;

        isSummoning = true;

        UpdateSummonPhase(); // 시작하자마자 현재 체력 기준 규칙 적용

        if (enemySpawner != null)
            enemySpawner.StartSpawn(); // 잡몹 소환 시작
    }

    public void StopSummon(bool clearEnemies)
    {
        isSummoning = false;

        if (enemySpawner == null)
            return;

        enemySpawner.StopSpawn(); // 추가 소환 중지

        if (clearEnemies)
            enemySpawner.KillAllSpawnedEnemies(); // 남은 잡몹 전부 풀로 반환
    }

    private void UpdateSummonPhase()
    {
        if (bossHealth == null)
            return;

        if (enemySpawner == null)
            return;

        BossSummonPhase nextPhase = GetPhaseByHealthPercent(bossHealth.GetHealthPercent());

        if (nextPhase == null)
            return;

        if (currentPhase == nextPhase)
            return; // 같은 구간이면 다시 적용하지 않음

        currentPhase = nextPhase;

        enemySpawner.SetSpawnRule(
            currentPhase.maxAliveCount,
            currentPhase.spawnInterval
        ); // 체력 구간에 맞게 스포너 규칙 변경
    }

    private BossSummonPhase GetPhaseByHealthPercent(float healthPercent)
    {
        if (summonPhases == null || summonPhases.Length == 0)
            return null;

        BossSummonPhase selectedPhase = null;

        for (int i = 0; i < summonPhases.Length; i++)
        {
            BossSummonPhase phase = summonPhases[i];

            if (phase == null)
                continue;

            if (healthPercent <= phase.healthPercent)
            {
                if (selectedPhase == null || phase.healthPercent < selectedPhase.healthPercent)
                    selectedPhase = phase; // 조건에 맞는 것 중 가장 낮은 체력 구간 선택
            }
        }

        return selectedPhase;
    }

    private void OnBossDied(EnemyHealth deadBoss)
    {
        StopSummon(true); //  보스가 죽으면 소환 중지 + 잡몹 전부 반환
    }
}