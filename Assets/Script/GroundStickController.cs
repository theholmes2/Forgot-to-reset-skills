using UnityEngine;

public class GroundStickController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rigid;
    public Collider2D bodyCollider;
    public LayerMask groundLayer;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.12f;
    public float groundGraceTime = 0.12f;

    [Header("Slope Launch Limit")]
    public float maxGroundedUpwardSpeed = 0.5f;

    private float lastGroundedTime = float.MinValue;
    private float allowUpwardVelocityUntil = float.MinValue;

    private void Awake()
    {
        if (rigid == null)
            rigid = GetComponent<Rigidbody2D>();

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (rigid == null || bodyCollider == null)
            return;

        if (IsGrounded())
            lastGroundedTime = Time.time;

        if (Time.time < allowUpwardVelocityUntil)
            return;

        if (Time.time > lastGroundedTime + groundGraceTime)
            return;

        if (rigid.linearVelocity.y <= maxGroundedUpwardSpeed)
            return;

        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, maxGroundedUpwardSpeed);
    }

    public void AllowUpwardVelocity(float duration)
    {
        allowUpwardVelocityUntil = Time.time + duration;
    }

    public bool IsGrounded()
    {
        Bounds bounds = bodyCollider.bounds;

        Vector2 checkOrigin = new Vector2(bounds.center.x, bounds.min.y + 0.03f);
        Vector2 checkSize = new Vector2(bounds.size.x * 0.75f, 0.05f);

        RaycastHit2D hit = Physics2D.BoxCast(checkOrigin, checkSize, 0f, Vector2.down, groundCheckDistance, groundLayer);

        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (bodyCollider == null)
            return;

        Bounds bounds = bodyCollider.bounds;

        Vector3 checkCenter = new Vector3(bounds.center.x, bounds.min.y + 0.03f - groundCheckDistance * 0.5f, transform.position.z);
        Vector3 checkSize = new Vector3(bounds.size.x * 0.75f, groundCheckDistance, 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(checkCenter, checkSize);
    }
}