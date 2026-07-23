using UnityEngine;

public class PooledEnemy : MonoBehaviour
{
    private EnemyPool ownerPool; // 나를 만든 풀

    public void SetOwnerPool(EnemyPool pool)
    {
        ownerPool = pool; // 풀 기억
    }

    public void ReturnToPool()
    {
        if (ownerPool != null)
        {
            ownerPool.ReturnEnemy(gameObject); // 풀로 반환
            return;
        }

        gameObject.SetActive(false); // 풀이 없으면 그냥 비활성화
    }
}