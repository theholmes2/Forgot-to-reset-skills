using UnityEngine;

public class SpawnAreaController : MonoBehaviour
{
    public EnemySpawner enemySpawner; // 이 구역에서 사용할 스포너

    public bool startSpawnOnEnter = true; // 플레이어가 들어오면 스폰 시작
    public bool stopSpawnOnExit = true; // 플레이어가 나가면 스폰 중지
    public bool clearEnemiesOnExit = true; // 플레이어가 나가면 남은 몹 제거

    private void Awake()
    {
        if (enemySpawner == null)
            enemySpawner = GetComponentInChildren<EnemySpawner>(); // 자식 스포너 자동 찾기
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (enemySpawner == null)
            return;

        if (startSpawnOnEnter)
            enemySpawner.StartSpawn(); // 구역 진입 시 스폰 시작
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (enemySpawner == null)
            return;

        if (stopSpawnOnExit)
            enemySpawner.StopSpawn(); // 구역 이탈 시 추가 스폰 중지

        if (clearEnemiesOnExit)
            enemySpawner.KillAllSpawnedEnemies(); // 구역 이탈 시 기존 몹 전부 풀로 반환
    }
}