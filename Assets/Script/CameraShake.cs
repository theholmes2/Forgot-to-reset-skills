using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Vector3 CurrentOffset { get; private set; } // 현재 흔들림 값

    private Coroutine shakeCoroutine; // 흔들림 중복 방지용

    public void Shake(float duration, float power)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine); // 이미 흔들리는 중이면 기존 흔들림 중지

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, power));
    }

    private IEnumerator ShakeRoutine(float duration, float power)
    {
        float timer = 0f;

        while (timer < duration)
        {
            float x = Random.Range(-1f, 1f) * power;
            float y = Random.Range(-1f, 1f) * power;

            CurrentOffset = new Vector3(x, y, 0f); // 위치를 직접 바꾸지 않고 흔들림 값만 저장

            timer += Time.deltaTime;
            yield return null;
        }

        CurrentOffset = Vector3.zero; // 흔들림 끝나면 오프셋 제거
        shakeCoroutine = null;
    }
}