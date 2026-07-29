using UnityEngine;

public class FallDeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어인지 확인
        if (!collision.CompareTag("Player"))
            return;

        // 플레이어 체력 스크립트 찾기
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            playerHealth = collision.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        // 기존 사망/회귀 흐름 실행
        playerHealth.ForceDie();
    }
}