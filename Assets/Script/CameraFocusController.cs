using System.Collections;
using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    public CameraShake cameraShake; // 흔들림 값 받아오기

    private void Awake()
    {
        if (cameraShake == null)
            cameraShake = GetComponent<CameraShake>(); // 같은 카메라에 붙어있는 Shake 자동 찾기
    }

    public IEnumerator MoveToTarget(Transform target, float moveTime)
    {
        if (target == null)
            yield break;

        Vector3 startPosition = transform.position; // 현재 카메라 위치
        Vector3 targetPosition = target.position; // 이동할 대상 위치

        targetPosition.z = startPosition.z; // 카메라 z값 유지

        float timer = 0f;

        while (timer < moveTime)
        {
            timer += Time.deltaTime;

            float t = timer / moveTime;
            t = Mathf.SmoothStep(0f, 1f, t); // 부드럽게 이동

            Vector3 basePosition = Vector3.Lerp(startPosition, targetPosition, t);
            transform.position = basePosition + GetShakeOffset(); // 이동 위치 + 흔들림

            yield return null;
        }

        transform.position = targetPosition + GetShakeOffset(); // 마지막 위치 보정
    }

    public IEnumerator FollowTargetForSeconds(Transform target, float duration, float followSpeed)
    {
        if (target == null)
            yield break;

        float timer = 0f;

        while (timer < duration)
        {
            Vector3 targetPosition = target.position;
            targetPosition.z = transform.position.z; // 카메라 z값 유지

            Vector3 basePosition = Vector3.Lerp(
                transform.position - GetShakeOffset(),
                targetPosition,
                followSpeed * Time.deltaTime
            ); // 흔들림을 뺀 실제 위치 기준으로 따라가기

            transform.position = basePosition + GetShakeOffset(); // 따라가기 + 흔들림

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private Vector3 GetShakeOffset()
    {
        if (cameraShake == null)
            return Vector3.zero;

        return cameraShake.CurrentOffset; // 현재 흔들림 값 반환
    }
}