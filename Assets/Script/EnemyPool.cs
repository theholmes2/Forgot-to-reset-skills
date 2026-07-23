using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public GameObject enemyPrefab; // 풀에서 관리할 적 프리팹
    public int initialCount = 5; // 처음에 미리 만들어둘 개수
    public bool canExpand = true; // 부족하면 추가 생성할지

    private readonly Queue<GameObject> pool = new Queue<GameObject>(); // 비활성 적 보관소

    private void Awake()
    {
        CreateInitialEnemies(); // 시작할 때 미리 생성
    }

    private void CreateInitialEnemies()
    {
        if (enemyPrefab == null)
            return;

        for (int i = 0; i < initialCount; i++)
        {
            GameObject enemy = CreateEnemy();
            ReturnEnemy(enemy); // 만든 뒤 바로 풀에 넣기
        }
    }

    private GameObject CreateEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, transform); // 풀 오브젝트 밑에 생성

        PooledEnemy pooledEnemy = enemy.GetComponent<PooledEnemy>();

        if (pooledEnemy == null)
            pooledEnemy = enemy.AddComponent<PooledEnemy>(); // 풀 반환용 컴포넌트 자동 추가

        pooledEnemy.SetOwnerPool(this); // 이 풀에서 나온 적이라고 기억

        enemy.SetActive(false);
        return enemy;
    }

    public GameObject GetEnemy(Vector3 position, Quaternion rotation)
    {
        GameObject enemy = null;

        if (pool.Count > 0)
        {
            enemy = pool.Dequeue(); // 풀에 있는 적 꺼내기
        }
        else if (canExpand)
        {
            enemy = CreateEnemy(); // 부족하면 새로 생성
        }

        if (enemy == null)
            return null;

        enemy.transform.SetParent(null); // 씬에서 독립적으로 움직이게 분리
        enemy.transform.position = position;
        enemy.transform.rotation = rotation;

        Enemy enemyComponent = enemy.GetComponent<Enemy>();

        if (enemyComponent != null)
        {
          
            enemyComponent.ResetEnemy(); // 체력, 콜라이더, 상태 초기화
        }
        else
        {
            enemy.SetActive(true); // Enemy가 없으면 그냥 활성화
        }

        return enemy;
    }

    public void ReturnEnemy(GameObject enemy)
    {
        if (enemy == null)
            return;


        enemy.SetActive(false); // 비활성화해서 화면에서 제거
        enemy.transform.SetParent(transform); // 풀 밑으로 정리
        pool.Enqueue(enemy); // 다시 사용 가능하게 저장
        Debug.Log("몹 저장됨");
    }
}