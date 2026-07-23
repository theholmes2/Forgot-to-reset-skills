using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool")]
    public EnemyPool enemyPool; // 몹을 꺼낼 풀

    [Header("Spawn Point")]
    public Transform[] spawnPoints; // 몹이 나올 위치들

    [Header("Spawn Rule")]
    public float spawnInterval = 3f; // 몇 초마다 생성할지
    public int maxAliveCount = 5; // 동시에 살아있을 수 있는 최대 몹 수
    public bool spawnOnStart; // 시작하자마자 스폰할지

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>(); // 이 스포너가 꺼낸 몹들

    private Coroutine spawnRoutine;
    private bool isSpawning;

    public bool IsSpawning => isSpawning; //  외부에서 스폰 중인지 확인

    private void Start()
    {
        if (spawnOnStart)
            StartSpawn(); // 테스트용 자동 스폰
    }

    public void StartSpawn()
    {
        if (isSpawning)
            return; // 중복 실행 방지

        isSpawning = true;
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawn()
    {
        isSpawning = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void ClearAndStop() //  스폰 중지 + 기존 몹 전체 반환
    {
        StopSpawn();
        KillAllSpawnedEnemies();
    }

    public void SetSpawnRule(int newMaxAliveCount, float newSpawnInterval) // 보스 체력 비율에 따라 스폰 규칙 변경
    {
        maxAliveCount = newMaxAliveCount;
        spawnInterval = newSpawnInterval;
    }

    public int GetAliveCount() // 현재 이 스포너가 관리 중인 살아있는 몹 수
    {
        RemoveInactiveEnemies();
        return spawnedEnemies.Count;
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            RemoveInactiveEnemies(); // 죽어서 풀로 돌아간 몹 정리

            if (CanSpawn())
                SpawnEnemy();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private bool CanSpawn()
    {
        if (enemyPool == null)
            return false; // 풀이 없으면 스폰 불가

        if (spawnPoints == null || spawnPoints.Length == 0)
            return false; // 스폰 위치 없으면 불가

        if (spawnedEnemies.Count >= maxAliveCount)
            return false; // 최대 개체 수 제한

        return true;
    }

    private void SpawnEnemy()
    {
        Transform spawnPoint = GetRandomSpawnPoint();

        if (spawnPoint == null)
            return;

        GameObject enemy = enemyPool.GetEnemy(spawnPoint.position, spawnPoint.rotation);

        if (enemy == null)
            return;

        spawnedEnemies.Add(enemy); // 이 스포너가 관리하는 몹으로 기억
    }

    private Transform GetRandomSpawnPoint()
    {
        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index];
    }

    private void RemoveInactiveEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null || !spawnedEnemies[i].activeSelf)
                spawnedEnemies.RemoveAt(i); // 풀로 돌아간 몹 제거
        }
    }

    public void KillAllSpawnedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
                continue;

            Enemy enemy = spawnedEnemies[i].GetComponent<Enemy>();

            if (enemy == null)
                continue; // Enemy가 없으면 잘못된 프리팹이므로 무시

            enemy.ForceDead(); // 죽음 애니메이션 후 풀로 반환
        }

        spawnedEnemies.Clear(); // 스포너 관리는 끊음
    }
}