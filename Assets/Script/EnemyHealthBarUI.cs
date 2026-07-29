using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    public EnemyHealth enemyHealth; // 체력 정보
    public Image fillImage; // 체력바 채워지는 이미지

    public GameObject root; // 체력바 전체 오브젝트
    public bool hideWhenFull = true; // 체력이 가득 차면 숨길지

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>(); // 부모에서 체력 찾기
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged += UpdateHealthBar; // 체력 변화 감지
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged -= UpdateHealthBar; // 이벤트 해제
    }

    private void Start()
    {
        if (enemyHealth != null)
            UpdateHealthBar(enemyHealth.currentHealth, enemyHealth.maxHealth); // 시작 시 한 번 갱신
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (fillImage == null)
            return;

        float ratio = 0f;

        if (maxHealth > 0f)
            ratio = currentHealth / maxHealth;

        fillImage.fillAmount = ratio; // 체력 비율 반영

        if (root != null)
        {
            bool shouldShow = true;

            if (hideWhenFull && ratio >= 1f)
                shouldShow = false; // 풀피면 숨김

            if (ratio <= 0f)
                shouldShow = false; // 죽으면 숨김

            root.SetActive(shouldShow);
        }
    }
}