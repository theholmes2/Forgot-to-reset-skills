using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visual;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int LandHash = Animator.StringToHash("Land");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private bool wasGrounded = true;
    private float ignoreGroundedUntil;

    private const string AttackLockKey = "Attack";

    private PlayerControlLock controlLock;
    private Rigidbody2D rigid;

    private bool isDead;
    private bool isAttacking;
    private bool isFacingRight = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visual == null && animator != null)
            visual = animator.transform;

        controlLock = GetComponent<PlayerControlLock>();
        rigid = GetComponent<Rigidbody2D>();
    }

    public void SetMovement(float horizontalSpeed)
    {
        if (isDead || isAttacking)
            return;

        animator.SetFloat(SpeedHash, Mathf.Abs(horizontalSpeed));

        if (horizontalSpeed > 0f)
            SetFacing(true);
        else if (horizontalSpeed < 0f)
            SetFacing(false);
    }

    public void PlayAttack()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        animator.SetFloat(SpeedHash, 0f);

        if (rigid != null)
            rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);

        if (controlLock != null)
            controlLock.Lock(AttackLockKey);

        animator.SetTrigger(AttackHash);
    }

    public void EndAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;

        if (controlLock != null)
            controlLock.Unlock(AttackLockKey);
    }

    public void PlayHit()
    {
        if (isDead)
            return;

        EndAttack();
        animator.SetTrigger(HitHash);
    }

    public void PlayDie()
    {
        if (isDead)
            return;

        EndAttack();

        isDead = true;
        animator.SetFloat(SpeedHash, 0f);
        animator.SetTrigger(DieHash);
    }

    private void SetFacing(bool faceRight)
    {
        if (visual == null || isFacingRight == faceRight)
            return;

        Vector3 scale = visual.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        visual.localScale = scale;
        isFacingRight = faceRight;
    }

    private void OnDisable()
    {
        if (controlLock != null)
            controlLock.Unlock(AttackLockKey);
    }

    public void PlayJump()
    {
        if (isDead)
            return;

        ignoreGroundedUntil = Time.time + 0.1f;
        wasGrounded = false;

        animator.ResetTrigger(LandHash);
        animator.SetTrigger(JumpHash);
    }

    public void SetAirState(float verticalSpeed, bool isGrounded)
    {
        if (isDead)
            return;

        if (Time.time < ignoreGroundedUntil)
            isGrounded = false;

        animator.SetFloat(VerticalSpeedHash, verticalSpeed);
        animator.SetBool(IsGroundedHash, isGrounded);

        if (!wasGrounded && isGrounded)
        {
            animator.ResetTrigger(JumpHash);
            animator.SetTrigger(LandHash);
        }

        wasGrounded = isGrounded;
    }

}