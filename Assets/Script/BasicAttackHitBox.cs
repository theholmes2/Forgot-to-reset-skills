using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackHitBox : MonoBehaviour
{

    public BoxCollider2D coll;
    private SpriteRenderer spriteRenderer;

    public float startDelay = 0.2f; // 공격 판정이 켜지기 전 시간
    public float activeTime = 0.2f;  // 공격 판정이 유지되는 시간
    public float damage;
    

    private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>(); //이 공격을 맞은 적 목록

    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>(); 
        coll.enabled = false; //판정off
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }
    private void Start()
    {
        StartCoroutine(AttackLife());

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("적을 공격했다! 데미지: " + damage);
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
           
            if (enemyHealth != null)
            {
                if (hitEnemies.Contains(enemyHealth))// 이미 hitEnemies에 enemyHealth가 들어있다면 
                    return;

                // hitEnemies에 enemyHealth 추가
                hitEnemies.Add(enemyHealth);
                // enemyHealth에게 damage 전달
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    IEnumerator AttackLife()
    {

        yield return new WaitForSeconds(startDelay); //선딜
        coll.enabled = true;// 판정 on
        spriteRenderer.enabled = true;
        yield return new WaitForSeconds(activeTime); //공격판정시간
        EndAttack(); //공격 끝
    }
    private void EndAttack()
    {
        Destroy(gameObject);
        // 지금은 Destroy
        // 나중에는 PoolManager에 반환
    }
    public void Init(float skillDamage)
    {
        // 데미지 받아옴
        damage = skillDamage;
    }
}