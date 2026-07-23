using System.Collections;
using UnityEngine;

public class RestrictionWall : MonoBehaviour
{
    public Collider2D wallCollider; // 실제로 막는 콜라이더
    public SpriteRenderer wallRenderer; // 벽 기본 표시용 이미지

    [Header("Wall Color")]
    public Color idleColor = new Color(0.2f, 0.8f, 1f, 0.08f); // 평소 거의 투명한 색
    public Color hitColor = new Color(0.6f, 1f, 1f, 0.25f); // 부딪혔을 때 벽 전체가 살짝 밝아지는 색

    [Header("Glow Effect")]
    public GameObject contactGlowPrefab; // 부딪힌 위치에 생길 빛 이펙트
    public float glowTime = 0.25f; // 벽 전체가 밝아졌다 돌아오는 시간

    public float contactGlowInterval = 0.08f; // 빛 생성 간격
    public float GlowYOffset = 0.3f;
    private float lastGlowTime; // 마지막 생성 시간

    private Coroutine glowRoutine;

    private void Awake()
    {
        if (wallCollider == null)
            wallCollider = GetComponent<Collider2D>(); // 콜라이더 자동 찾기

        if (wallRenderer == null)
            wallRenderer = GetComponent<SpriteRenderer>(); // 스프라이트 자동 찾기

        SetWallActive(false); // 시작할 때는 꺼둠
    }

    public void SetWallActive(bool isActive)
    {
        if (wallCollider != null)
            wallCollider.enabled = isActive; // 실제 벽 충돌 켜기/끄기

        if (wallRenderer != null)
        {
            wallRenderer.enabled = isActive; // 벽 이미지 켜기/끄기
            wallRenderer.color = idleColor; // 기본 색으로 초기화
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return; // 플레이어가 아니면 무시

        Vector2 contactPoint = collision.GetContact(0).point; // 부딪힌 위치
        contactPoint.y = contactPoint.y + GlowYOffset;
        ShowContactGlow(contactPoint);
        PlayWallGlow();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return; // 플레이어가 아니면 무시

        Vector2 contactPoint = collision.GetContact(0).point; // 계속 닿고 있는 위치
        contactPoint.y = contactPoint.y + GlowYOffset;
        ShowContactGlow(contactPoint);
        PlayWallGlow();
    }

    private void ShowContactGlow(Vector2 contactPoint)
    {
        if (contactGlowPrefab == null)
            return;

        if (Time.time < lastGlowTime + contactGlowInterval)
            return; // 너무 자주 생성되는 것 방지

        lastGlowTime = Time.time;

        Instantiate(contactGlowPrefab, contactPoint, Quaternion.identity); // 충돌 위치에 빛 생성
    }

    private void PlayWallGlow()
    {
        if (wallRenderer == null)
            return;

        if (glowRoutine != null)
            StopCoroutine(glowRoutine); // 기존 빛남 중이면 다시 시작

        glowRoutine = StartCoroutine(WallGlowRoutine());
    }

    private IEnumerator WallGlowRoutine()
    {
        float timer = 0f;

        while (timer < glowTime)
        {
            timer += Time.deltaTime;

            float t = timer / glowTime;

            wallRenderer.color = Color.Lerp(hitColor, idleColor, t); // 밝았다가 서서히 사라짐

            yield return null;
        }

        wallRenderer.color = idleColor;
        glowRoutine = null;
    }

    private void Reset()
    {
        // 컴포넌트 붙이는 순간 자동 연결
        wallCollider = GetComponent<BoxCollider2D>();
        wallRenderer = GetComponent<SpriteRenderer>();

        FitColliderToSprite();
    }

    private void OnValidate()
    {
        // 인스펙터에서 값 바꿀 때 자동 갱신
        if (wallCollider == null)
            wallCollider = GetComponent<BoxCollider2D>();

        if (wallRenderer == null)
            wallRenderer = GetComponent<SpriteRenderer>();

        FitColliderToSprite();
    }

    private void FitColliderToSprite()
    {
        if (wallCollider == null)
            return;

        if (wallRenderer == null)
            return;

        if (wallRenderer.sprite == null)
            return;

        BoxCollider2D boxCollider = wallCollider as BoxCollider2D;

        if (boxCollider == null)
            return;

        // SpriteRenderer 기준 로컬 크기에 맞춤
        boxCollider.size = wallRenderer.sprite.bounds.size;
        boxCollider.offset = wallRenderer.sprite.bounds.center;
    }
}