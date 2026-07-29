using System.Collections;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    [Header("Portal")]
    public GameObject portalVisual; // 포탈 이미지/이펙트 오브젝트
    public SpriteRenderer portalRenderer; // 포탈 투명도 조절용
    public Collider2D portalCollider; // 포탈 진입 트리거

    [Header("Open Setting")]
    public float openTime = 2f; // 포탈이 완전히 열리는 시간

    [Header("Ending")]
    public GameObject endingPanel; // 플레이해주셔서 감사합니다 UI
    public bool hidePlayerOnEnter = true; // 포탈 진입 시 플레이어 숨길지
    public bool lockPlayerOnEnter = true; // 포탈 진입 시 플레이어 조작 막을지

    private bool isOpening; // 포탈이 열리는 중인지
    private bool isOpen; // 포탈이 완전히 열렸는지
    private Coroutine openRoutine;

    private void Awake()
    {
        if (portalRenderer == null && portalVisual != null)
            portalRenderer = portalVisual.GetComponent<SpriteRenderer>();

        if (portalCollider == null)
            portalCollider = GetComponent<Collider2D>();

        ClosePortal(); // 시작할 때 포탈 닫기
    }

    public void PortalOpen()
    {
        if (isOpen || isOpening)
            return; // 이미 열렸거나 열리는 중이면 중복 실행 방지

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        Debug.Log("포탈열기");
        openRoutine = StartCoroutine(PortalOpenRoutine());
    }

    private IEnumerator PortalOpenRoutine()
    {
        isOpening = true;
        isOpen = false;

        if (portalVisual != null)
            portalVisual.SetActive(true); // 포탈 보이기 시작

        if (portalCollider != null)
            portalCollider.enabled = false; // 완전히 열리기 전까지는 못 타게 함

        SetPortalAlpha(0f); // 처음엔 투명

        float timer = 0f;

        while (timer < openTime)
        {
            timer += Time.deltaTime;

            float ratio = timer / openTime;
            SetPortalAlpha(ratio); // 천천히 불투명해짐

            yield return null;
        }

        SetPortalAlpha(1f); // 완전히 보이게 고정

        isOpening = false;
        isOpen = true;
        openRoutine = null;

        if (portalCollider != null)
            portalCollider.enabled = true; // 이제 포탈 진입 가능
    }

    public void ClosePortal()
    {
        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }

        isOpening = false;
        isOpen = false;

        if (portalCollider != null)
            portalCollider.enabled = false; // 포탈 진입 불가

        SetPortalAlpha(0f);

        if (portalVisual != null)
            portalVisual.SetActive(false); // 포탈 숨김

        if (endingPanel != null)
            endingPanel.SetActive(false); // 엔딩 UI 숨김
    }

    private void SetPortalAlpha(float alpha)
    {
        if (portalRenderer == null)
            return;

        Color color = portalRenderer.color;
        color.a = alpha;
        portalRenderer.color = color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOpen)
            return; // 완전히 열린 뒤에만 진입 가능

        if (!collision.CompareTag("Player"))
            return;

        EnterPortal(collision.gameObject);
    }

    private void EnterPortal(GameObject playerObject)
    {
        if (portalCollider != null)
            portalCollider.enabled = false; // 중복 진입 방지

        if (lockPlayerOnEnter)
        {
            PlayerControlLock playerControlLock = playerObject.GetComponent<PlayerControlLock>();

            if (playerControlLock != null)
                playerControlLock.Lock("PortalEnter"); // 포탈 진입 후 조작 잠금
        }

        if (hidePlayerOnEnter)
            playerObject.SetActive(false); // 캐릭터 사라짐

        if (endingPanel != null)
            endingPanel.SetActive(true); // 엔딩 UI 표시
    }
}