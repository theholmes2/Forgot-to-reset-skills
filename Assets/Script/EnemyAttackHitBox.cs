using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHitBox : MonoBehaviour
{
    public BoxCollider2D coll;

    private float damage;
    private HashSet<PlayerHealth> hitPlayers = new();

    private void Awake()
    {
        if (coll == null)
            coll = GetComponent<BoxCollider2D>();

        if (coll != null)
            coll.enabled = false;
    }

    public void Init(float attackDamage, float activeTime)
    {
        damage = attackDamage;

        if (coll != null)
            coll.enabled = true;

        StartCoroutine(AttackLife(activeTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void TryDamagePlayer(Collider2D collision)
    {
        PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (hitPlayers.Contains(playerHealth))
            return;

        hitPlayers.Add(playerHealth);
        playerHealth.TakeDamage(damage);

        Debug.Log("적 공격 판정 데미지: " + damage);
    }

    private IEnumerator AttackLife(float activeTime)
    {
        yield return new WaitForSeconds(activeTime);

        Destroy(gameObject);
    }
}