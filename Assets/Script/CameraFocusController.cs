using System.Collections;
using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
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

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition; // 마지막 위치 보정
    }
}