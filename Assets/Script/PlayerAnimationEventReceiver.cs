using UnityEngine;

public class PlayerAnimationEventReceiver : MonoBehaviour
{
    private PlayerAnimationController animationController;

    private void Awake()
    {
        animationController = GetComponentInParent<PlayerAnimationController>();
    }

    public void EndAttack()
    {
        if (animationController != null)
            animationController.EndAttack();
    }
}