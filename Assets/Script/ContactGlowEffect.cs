using UnityEngine;

public class ContactGlowEffect : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // 빛 이미지
    public float lifeTime = 0.25f; // 유지 시간
    public float startScale = 0.4f; // 처음 크기
    public float endScale = 1.2f; // 사라질 때 크기

    private float timer;
    private Color startColor;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>(); // 자동 연결

        if (spriteRenderer != null)
            startColor = spriteRenderer.color; // 처음 색 저장

        transform.localScale = Vector3.one * startScale; // 시작 크기 적용
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = timer / lifeTime;

        // 시간이 지날수록 커짐
        float scale = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = Vector3.one * scale;

        // 시간이 지날수록 투명해짐
        if (spriteRenderer != null)
        {
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, t);
            spriteRenderer.color = color;
        }

        if (timer >= lifeTime)
            Destroy(gameObject); // 끝나면 삭제
    }
}