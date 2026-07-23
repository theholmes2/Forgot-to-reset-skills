using System.Collections;
using UnityEngine;

public class BossIntroDirector : MonoBehaviour
{
    public GameObject bossObject; // 등장시킬 보스
    public GameObject introPanel; // 쿠아아앙 같은 UI

    [Header("Intro Options")]
    public bool lockPlayerDuringIntro = true; // 등장 중 플레이어 조작 잠금
    public bool lockCameraDuringIntro = true; // 등장 중 카메라 고정
    public bool useCameraShake = true; // 등장 중 카메라 흔들림 사용
    public bool showIntroPanel = true; // 등장 문구 표시

    [Header("Camera Shake")]
    public CameraShake cameraShake; // 흔들림 연출
    public float shakeTime = 0.6f;
    public float shakePower = 0.15f;

    [Header("Timing")]
    public float introTime = 1.5f; // 등장 문구 유지 시간

    [Header("Camera Focus")]
    public bool moveCameraToBoss = true; // 등장 전에 카메라를 보스 쪽으로 이동
    public Transform cameraFocusTarget; // 카메라가 볼 위치
    public float cameraMoveTime = 0.6f; // 보스 위치로 이동하는 시간

    [Header("Camera Follow During Intro")]
    public bool followBossDuringIntro = true; // 등장 중 보스를 계속 따라갈지
    public float followBossTime = 1f; // 보스를 따라가는 시간
    public float followBossSpeed = 8f; // 보스를 따라가는 속도

    private void Awake()
    {
        if (cameraShake == null && Camera.main != null)
            cameraShake = Camera.main.GetComponent<CameraShake>(); // 카메라 흔들림 자동 찾기

        if (cameraFocusTarget == null && bossObject != null)
            cameraFocusTarget = bossObject.transform; // 기본 포커스는 보스 위치
    }

    public IEnumerator PlayIntro()
    {
        if (showIntroPanel && introPanel != null)
            introPanel.SetActive(true); // 글자 표시

        if (bossObject != null)
            bossObject.SetActive(true); // 보스 출현

        if (useCameraShake && cameraShake != null)
            cameraShake.Shake(shakeTime, shakePower); // 흔들림 시작

        yield return new WaitForSeconds(introTime); // 연출 유지

        if (introPanel != null)
            introPanel.SetActive(false); // 글자 사라짐
    }
}