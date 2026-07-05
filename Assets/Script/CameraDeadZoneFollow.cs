using UnityEngine;

public class CameraDeadZoneFollow : MonoBehaviour
{
    public Transform target; // 플레이어
    public Vector2 deadZoneSize = new Vector2(3f, 2f); // 카메라가 안 움직이는 범위
    public float followSpeed = 5f; // 따라가는 속도

    public bool isLocked; // true면 카메라 따라가기 정지

    private void LateUpdate()
    {
        if (isLocked)
            return; // 보스 등장 연출 중 카메라 고정

        if (target == null)
            return;

        Vector3 cameraPosition = transform.position;
        Vector3 targetPosition = target.position;

        float xDifference = targetPosition.x - cameraPosition.x;
        float yDifference = targetPosition.y - cameraPosition.y;

        Vector3 nextPosition = cameraPosition;

        if (Mathf.Abs(xDifference) > deadZoneSize.x * 0.5f)
        {
            float xOffset = Mathf.Sign(xDifference) * deadZoneSize.x * 0.5f;
            nextPosition.x = targetPosition.x - xOffset;
        }

        if (Mathf.Abs(yDifference) > deadZoneSize.y * 0.5f)
        {
            float yOffset = Mathf.Sign(yDifference) * deadZoneSize.y * 0.5f;
            nextPosition.y = targetPosition.y - yOffset;
        }

        nextPosition.z = cameraPosition.z;

        transform.position = Vector3.Lerp(
            cameraPosition,
            nextPosition,
            followSpeed * Time.deltaTime
        );
    }
}