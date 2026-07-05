using System.Collections;
using UnityEngine;

public class BossAnimationEventReceiver : MonoBehaviour
{
    public GameObject attackEffect; // 공격 이펙트 오브젝트
    public float effectLifeTime = 1f; // 이펙트 유지 시간

    private Coroutine effectCoroutine;

    private void Awake()
    {
        if (attackEffect != null)
            attackEffect.SetActive(false); // 시작할 때 꺼두기
    }

    public void ActivateAttackEffect1()
    {
        PlayAttackEffect();
    }

    public void ActivateAttackEffect2()
    {
        PlayAttackEffect();
    }

    private void PlayAttackEffect()
    {
        if (attackEffect == null)
            return;

        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        attackEffect.SetActive(false); // 기존 이펙트 초기화
        attackEffect.SetActive(true); // 현재 공격 타이밍에 다시 켜기

        effectCoroutine = StartCoroutine(EffectLifeRoutine());
    }

    private IEnumerator EffectLifeRoutine()
    {
        yield return new WaitForSeconds(effectLifeTime);

        if (attackEffect != null)
            attackEffect.SetActive(false); // 일정 시간 뒤 끄기

        effectCoroutine = null;
    }

    public void DieFinish()
    {
        Destroy(gameObject);
    }
}