using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    public float currentHealth;
    public Enemy enemy;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // currentHealth를 maxHealth로 초기화
        currentHealth = maxHealth;
        enemy = GetComponent<Enemy>(); 
    }

    public void TakeDamage(float damage)
    {
        if (enemy.currentState == Enemy.State.Dead) //죽은상태면 리턴
            return;

       
        // currentHealth에서 damage만큼 빼기
        currentHealth -= damage;
        
        // 현재 체력이 0 이하라면 Die 함수 호출
        if (currentHealth <= 0) {
            Die();
            return;
        }

        enemy.ChangeState(Enemy.State.Hit); //안죽었으면 맞은상태됨

    }
    private void Die()
    {
        enemy.ChangeState(Enemy.State.Dead);


    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
