using System;
using System.Collections;
using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    public float currentHealth; // 현재 체력

    public PlayerStatController playerStatController; // 최종 스탯 계산기
    public PlayerAnimationController animationController;
    private PlayerSoundController soundController;
    public event Action OnDied; // 플레이어 사망 알림
    public event Action<float, float> OnHealthChanged; // 현재 체력, 최대 체력
   
    private bool isDead;
    public bool IsDead
    {
        get { return isDead; } // 다른 스크립트가 죽었는지 확인용
    }
    private void Awake()
    {
        if (playerStatController == null)
            playerStatController = GetComponent<PlayerStatController>();

        currentHealth = GetMaxHealth(); // 시작 체력 초기화
        if (animationController == null)
            animationController = GetComponent<PlayerAnimationController>();

        soundController = GetComponentInChildren<PlayerSoundController>();

    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);
        NotifyHealthChanged();

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (animationController != null)
            animationController.PlayHit();
    }

    public void ForceDie()
    {
        if (isDead)
            return; // 이미 죽었으면 중복 처리 안 함

        currentHealth = 0f; // 체력을 0으로 만듦
        NotifyHealthChanged();
        Die(); // 기존 사망/회귀 흐름 사용
    }

    public void Heal(float amount)
    {
        if (isDead)
            return;

        currentHealth += amount;

        float maxHealth = GetMaxHealth();

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        NotifyHealthChanged();
    }

    private float GetMaxHealth()
    {
        if (playerStatController == null)
            return 100f;

        return playerStatController.GetFinalStat(StatType.MaxHp);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (animationController != null)
            animationController.PlayDie();

        if (soundController != null)
            soundController.PlayDeathSound();


        OnDied?.Invoke(); // 사망 알림
        StartCoroutine(DeathReturnRoutine()); // 잠깐 기다렸다가 회귀

    }
    private IEnumerator DeathReturnRoutine()
    {
        yield return new WaitForSecondsRealtime(2.5f); // 시간정지 상태여도 기다림

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance가 없어서 회귀할 수 없음");
            yield break;
        }

        Debug.Log("플레이어 사망 회귀 실행");
        GameManager.Instance.OnPlayerDeath(); // 회귀
    }

    public float GetHealthRate()
    {
        float maxHealth = GetMaxHealth();

        if (maxHealth <= 0f)
            return 0f;

        return currentHealth / maxHealth; // 1 = 풀피, 0 = 사망
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaximumHealth()
    {
        return GetMaxHealth();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, GetMaxHealth());
    }
}
