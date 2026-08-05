using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class SupportCharacterController : MonoBehaviour
{
    [Header("SPUM Animation")]
    public SPUM_Prefabs spum;

    [Header("Movement")]
    public Rigidbody2D rigid;
    public Collider2D bodyCollider;
    public float moveSpeed = 3f;
    public float attackRange = 0.5f;

    [Header("Attack")]
    public Transform attackPoint;

    [FormerlySerializedAs("attackSkillData")]
    public SkillData basicAttackSkillData;

    public CombatResolver combatResolver;
    public float attackCooldown = 2f;
    public float attackDuration = 0.8f;

    private Transform target;
    private EnemyHealth targetHealth;

    private bool isAssisting;
    private bool isMoving;
    private bool isAttacking;
    private float lastAttackTime;
    
    [Header("Target")]
    public float retargetInterval = 0.5f;

    private float nextRetargetTime;
    
    private Collider2D targetCollider;

    [Header("Stage")]
    public BossStageController bossStageController;

    [Header("Facing")]
    public Transform visualRoot;
    public bool startsFacingRight;

    private bool isFacingRight;
    private Vector3 initialVisualScale;
    private Vector3 initialAttackPointPosition;

    private void Awake()
    {
        if (spum == null)
            spum = GetComponent<SPUM_Prefabs>();

        if (rigid == null)
            rigid = GetComponent<Rigidbody2D>();

        if (combatResolver == null)
            combatResolver = GetComponent<CombatResolver>();

        if (attackPoint == null)
            attackPoint = transform.Find("AttackPoint");

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        //if (bossStageController == null)
        //    bossStageController = FindAnyObjectByType<BossStageController>();

        if (visualRoot == null)
            visualRoot = transform.Find("UnitRoot");

        if (visualRoot != null)
            initialVisualScale = visualRoot.localScale;

        if (attackPoint != null)
            initialAttackPointPosition = attackPoint.localPosition;

        isFacingRight = startsFacingRight;

    }

    private void Start()
    {
        if (spum == null)
            return;

        spum.PopulateAnimationLists();
        spum.OverrideControllerInit();
        spum.GoIdleAnimation();
    }

    private void Update()
    {
        if (!isAssisting)
            return;

        if (bossStageController != null && bossStageController.currentState != BossStageState.Battle)
        {
            StopAssist();
            return;
        }

        if (isAttacking)
            return;

        if (Time.time >= nextRetargetTime)
        {
            nextRetargetTime = Time.time + retargetInterval;
            FindClosestEnemy();
        }

        if (!HasValidTarget())
        {
            StopAssist();
            return;
        }
        LookAtTarget();

        
        float distance = GetDistanceToTarget();

        if (distance > attackRange)
        {
            MoveToTarget();
            return;
        }

        StopMoving();

        if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
            StartCoroutine(AttackRoutine());
    }

    private void LookAtTarget()
    {
        if (target == null)
            return;

        bool targetIsRight = target.position.x > transform.position.x;
        SetFacing(targetIsRight);
    }

    private void SetFacing(bool faceRight)
    {
        if (visualRoot == null)
            return;

        if (isFacingRight == faceRight)
            return;

        float flipMultiplier = faceRight == startsFacingRight ? 1f : -1f;

        Vector3 visualScale = initialVisualScale;
        visualScale.x *= flipMultiplier;
        visualRoot.localScale = visualScale;

        if (attackPoint != null)
        {
            Vector3 attackPosition = initialAttackPointPosition;
            attackPosition.x *= flipMultiplier;
            attackPoint.localPosition = attackPosition;
        }

        isFacingRight = faceRight;
    }

    private void FindClosestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        EnemyHealth closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (EnemyHealth enemyHealth in enemies)
        {
            if (enemyHealth == null || !enemyHealth.gameObject.activeInHierarchy)
                continue;

            if (enemyHealth.currentHealth <= 0f)
                continue;

            float distance = Mathf.Abs(enemyHealth.transform.position.x - transform.position.x);

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestEnemy = enemyHealth;
        }

        if (closestEnemy == null)
            return;

        if (closestEnemy == targetHealth)
            return;

        SetTarget(closestEnemy);
    }

    private void SetTarget(EnemyHealth newTarget)
    {
        UnsubscribeTarget();

        targetHealth = newTarget;
        target = newTarget != null ? newTarget.transform : null;
        targetCollider = newTarget != null ? newTarget.GetComponentInChildren<Collider2D>() : null;

        if (targetHealth != null)
            targetHealth.OnDied += OnTargetDied;
    }
    private bool HasValidTarget()
    {
        if (target == null)
            return false;

        if (!target.gameObject.activeInHierarchy)
            return false;

        if (targetHealth != null && targetHealth.currentHealth <= 0f)
            return false;

        return true;
    }
    private float GetDistanceToTarget()
    {
        if (bodyCollider != null && targetCollider != null)
        {
            ColliderDistance2D colliderDistance = bodyCollider.Distance(targetCollider);

            if (colliderDistance.isOverlapped)
                return 0f;

            return colliderDistance.distance;
        }

        if (target == null)
            return float.MaxValue;

        return Mathf.Abs(target.position.x - transform.position.x);
    }
    public void BeginAssist(Transform assistTarget)
    {
        isAssisting = true;
        isAttacking = false;
        lastAttackTime = Time.time - attackCooldown;
        nextRetargetTime = 0f;

        EnemyHealth initialTarget = assistTarget != null ? assistTarget.GetComponent<EnemyHealth>() : null;

        if (initialTarget != null)
            SetTarget(initialTarget);

        FindClosestEnemy();
    }

    private void MoveToTarget()
    {
        if (target == null || rigid == null)
            return;

        float direction = Mathf.Sign(target.position.x - transform.position.x);
        rigid.linearVelocity = new Vector2(direction * moveSpeed, rigid.linearVelocity.y);

        if (isMoving)
            return;

        isMoving = true;

        if (spum != null)
            spum.PlayAnimation(PlayerState.MOVE, 0);
    }

    private void StopMoving()
    {
        if (rigid != null)
            rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);

        if (!isMoving)
            return;

        isMoving = false;

        if (spum != null && !isAttacking)
            spum.GoIdleAnimation();
    }

    private IEnumerator AttackRoutine()
    {
        if (isAttacking)
            yield break;

        isAttacking = true;
        lastAttackTime = Time.time;

        StopMoving();

        if (spum != null)
            spum.PlayAnimation(PlayerState.ATTACK, 0);

        SpawnAttackHitBox();

        yield return new WaitForSeconds(attackDuration);

        EndAttack();
    }

    public void SpawnAttackHitBox()
    {
        if (!isAttacking)
            return;

        if (basicAttackSkillData == null || basicAttackSkillData.skillPrefab == null)
            return;

        if (attackPoint == null || combatResolver == null)
            return;

        
        GameObject attackObject = Instantiate(basicAttackSkillData.skillPrefab, attackPoint.position, basicAttackSkillData.skillPrefab.transform.rotation);
        BasicAttackHitBox hitBox = attackObject.GetComponent<BasicAttackHitBox>();

        if (hitBox == null)
        {
            Debug.LogWarning("지원 캐릭터 공격 프리팹에 BasicAttackHitBox가 없습니다.");
            Destroy(attackObject);
            return;
        }

        float finalDamage = combatResolver.GetFinalAttackDamage(basicAttackSkillData);
        hitBox.Init(finalDamage, transform);

        if (GameSoundController.Instance != null)
            GameSoundController.Instance.PlaySupportAttack();
    }

    public void EndAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;

        if (spum != null)
            spum.GoIdleAnimation();
    }

    public void StopAssist()
    {
        isAssisting = false;
        isAttacking = false;

        StopAllCoroutines();
        StopMoving();
        UnsubscribeTarget();

        target = null;
        targetHealth = null;
        targetCollider = null;

        if (spum != null)
            spum.GoIdleAnimation();
    }

    private void OnTargetDied(EnemyHealth deadTarget)
    {
        UnsubscribeTarget();

        target = null;
        targetHealth = null;
        targetCollider = null;
        nextRetargetTime = 0f;

        StopMoving();
    }

    private void UnsubscribeTarget()
    {
        if (targetHealth != null)
            targetHealth.OnDied -= OnTargetDied;
    }

    private void OnDisable()
    {
        StopAssist();
    }
}
