using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition; // 원래 카메라 위치
    private Coroutine shakeCoroutine; // 흔들림 중복 방지용

    public void Shake(float duration, float power)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine); // 이미 흔들리는 중이면 멈춤

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, power));
    }

    private IEnumerator ShakeRoutine(float duration, float power)
    {
        originalPosition = transform.position; // 시작 위치 저장

        float timer = 0f;

        while (timer < duration)
        {
            float x = Random.Range(-1f, 1f) * power;
            float y = Random.Range(-1f, 1f) * power;

            transform.position = originalPosition + new Vector3(x, y, 0f); // 랜덤 흔들림

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition; // 끝나면 원래 위치로 복구
        shakeCoroutine = null;
    }
}