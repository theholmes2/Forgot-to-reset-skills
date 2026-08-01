using System.Collections.Generic;
using UnityEngine;

public class ProjectileSkillHitBox : MonoBehaviour
{
    public float speed = 8f; // 날아가는 속도
    public float lifeTime = 1.2f; // 일정 시간 뒤 사라짐
    public float fadeTime = 0.3f; // 사라지기 전 흐려지는 시간

    public float damage; // 데미지
    public string skillId; // 나중에 광폭화 체크용
    public Transform owner; // 이 스킬을 쓴 대상

    private Vector3 moveDirection; // 이동 방향
    private float timer; // 살아있는 시간
    private SpriteRenderer[] renderers; // 자식 포함 스프라이트들

    private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>(); // 이미 맞은 적 기록

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(); // 흐려지게 할 스프라이트들
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime; // 앞으로 전진

        timer += Time.deltaTime;

        if (timer >= lifeTime - fadeTime)
        {
            FadeOut(); // 사라지기 직전 흐려짐
        }

        if (timer >= lifeTime)
        {
            Destroy(gameObject); // 수명 끝나면 삭제
        }
    }

    public void Init(float skillDamage, Transform skillOwner, string usedSkillId)
    {
        damage = skillDamage;
        owner = skillOwner;
        skillId = usedSkillId;

        if (owner != null)
        {
            Transform facingRoot = owner.Find("FacingRoot");
            bool isFacingRight = facingRoot == null || facingRoot.lossyScale.x > 0f;

            moveDirection = isFacingRight ? Vector3.right : Vector3.left;

            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                renderer.flipY = !isFacingRight;
            }
        }
        else
        {
            moveDirection = transform.right;
        }
    }

    private void FadeOut()
    {
        float fadeStartTime = lifeTime - fadeTime;
        float fadeRatio = (timer - fadeStartTime) / fadeTime;
        float alpha = Mathf.Lerp(1f, 0f, fadeRatio);

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth enemyHealth = collision.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
            return;

        if (hitEnemies.Contains(enemyHealth))
            return; // 같은 적 중복 타격 방지

        hitEnemies.Add(enemyHealth);
        enemyHealth.TakeDamage(damage, owner, skillId); // 투사체가 어떤 스킬인지 같이 전달


    }
}