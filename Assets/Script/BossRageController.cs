using UnityEngine;

public class BossRageController : MonoBehaviour
{
    public string rageSkillId = "skill_fire_slash"; // 이 스킬에 맞으면 광폭화
    public Color rageColor = new Color(1f, 0.35f, 0.35f, 1f); // 광폭화 색상

    public float attackCooldownMultiplier = 0.3f; // 공격 쿨타임 배율, 0.5면 2배 빠름

    private bool isRage; // 이미 광폭화 되었는지
    private SpriteRenderer[] spriteRenderers; // 보스 전체 스프라이트
    private EnemyAttackController attackController; // 공격 쿨타임 조절용

    private float originalAttackCooldown; // 원래 공격 쿨타임 저장

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        attackController = GetComponent<EnemyAttackController>();

        if (attackController != null)
        {
            originalAttackCooldown = attackController.attackCooldown; // 원래 쿨타임 기억
        }
    }

    public void CheckRage(string hitSkillId)
    {
        if (isRage)
            return; // 이미 광폭화면 다시 실행 안 함

        if (string.IsNullOrEmpty(hitSkillId))
            return; // 일반공격처럼 스킬 id가 없으면 무시

        if (hitSkillId != rageSkillId)
            return; // 광폭화 조건 스킬이 아니면 무시

        EnterRage();
    }

    private void EnterRage()
    {
        isRage = true;

        SetBossColor(rageColor); // 보스 빨갛게 변경

        if (attackController != null)
        {
            // 공격 쿨타임 감소
            attackController.attackCooldown = originalAttackCooldown * attackCooldownMultiplier;
        }

        Debug.Log("보스 광폭화!");
    }

    private void SetBossColor(Color color)
    {
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer == null)
                continue;

            renderer.color = color;
        }
    }
}