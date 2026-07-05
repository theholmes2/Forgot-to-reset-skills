using UnityEngine;

public class BossProjectileMover : MonoBehaviour
{
    public float moveSpeed = 8f; // 투사체 이동 속도
    public float lifeTime = 2f; // 자동으로 꺼지는 시간

    private float timer;

    private Vector3 startLocalPosition; // 처음 로컬 위치
    private Quaternion startLocalRotation; // 처음 로컬 회전

    private void Awake()
    {
        startLocalPosition = transform.localPosition; // 처음 위치 저장
        startLocalRotation = transform.localRotation; // 처음 회전 저장
    }

    private void OnEnable()
    {
        timer = 0f;

        transform.localPosition = startLocalPosition; // 다시 발사될 때 위치 복구
        transform.localRotation = startLocalRotation; // 다시 발사될 때 회전 복구

        ParticleSystem particle = GetComponent<ParticleSystem>();

        if (particle != null)
        {
            particle.Clear(); // 이전 파티클 제거
            particle.Play(); // 새로 재생
        }
    }

    private void Update()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime, Space.Self); // 로컬 방향으로 이동

        timer += Time.deltaTime;

        if (timer >= lifeTime)
        {
            gameObject.SetActive(false); // 나중에 풀링 대비해서 삭제 대신 끄기
        }
    }
}