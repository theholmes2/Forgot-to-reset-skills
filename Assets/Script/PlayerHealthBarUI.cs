using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth; // 플레이어 체력 정보
    public Image fillImage; // 복사한 에너미 체력바의 Fill 이미지
    public GameObject root; // 숨김 처리할 체력바 표시 오브젝트. 이 스크립트가 붙은 오브젝트와 분리
    public bool hideWhenFull; // 플레이어 체력바는 기본적으로 계속 표시

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();

    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += Refresh;
    }

    private void Start()
    {
        if (playerHealth != null)
            Refresh(playerHealth.GetCurrentHealth(), playerHealth.GetMaximumHealth());
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= Refresh;
    }

    private void Refresh(float currentHealth, float maxHealth)
    {
        if (fillImage == null)
            return;

        float ratio = 0f;

        if (maxHealth > 0f)
            ratio = currentHealth / maxHealth;

        fillImage.fillAmount = ratio;

        if (root != null)
        {
            bool shouldShow = currentHealth > 0f;

            if (hideWhenFull && ratio >= 1f)
                shouldShow = false;

            root.SetActive(shouldShow);
        }
    }
}
