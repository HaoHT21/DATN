using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BossHeath : MonoBehaviour
{
    public int currentHeath = 500;
    public int maxHeath = 500;

    [Header("UI Health Bar")]
    public Image healthFill;

    [Header("Armor")]
    public int armor = 20; // giáp giảm damage

    [Header("UI Root (Panel chứa thanh máu)")]
    public Image healthUIRoot;

    private bool isDead;

    private float targetFill;
    public float smoothSpeed = 8f;

    private Image FindImageByName(string imageName)
    {
        Image[] images =
            Resources.FindObjectsOfTypeAll<Image>();

        foreach (Image img in images)
        {
            if (img.name == imageName)
            {
                Debug.Log("Tìm thấy Image: " + img.name);
                return img;
            }
        }

        Debug.LogError("Không tìm thấy Image: " + imageName);
        return null;
    }
    void Awake()
    {
        Debug.Log("===== AUTO FIND UI =====");

        healthUIRoot = FindImageByName("BossHealthBar");
        healthFill = FindImageByName("BossHealthFill");

        if (healthUIRoot != null)
            Debug.Log("Đã gán BossHealthBar");

        if (healthFill != null)
            Debug.Log("Đã gán BossHealthFill");
    }

    void Start()
    {
        targetFill = 1f;

        if (healthFill != null)
            healthFill.fillAmount = 1f;

        // ẩn UI lúc đầu
        if (healthUIRoot != null)
            healthUIRoot.enabled = false;
        if (healthFill != null)
            healthFill.enabled = false;
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHeath += amount;

        if (currentHeath > maxHeath)
            currentHeath = maxHeath;

        targetFill = (float)currentHeath / maxHeath;
    }

    public void TakeDamage(int damage, string dealerTag)
    {
        if (isDead) return;

        // =========================
        // GIÁP GIẢM DAMAGE
        // =========================
        int finalDamage = damage - armor;

        // không cho damage âm
        if (finalDamage < 1)
            finalDamage = 1;

        currentHeath -= finalDamage;

        // Debug để test
        Debug.Log($"Boss bị đánh: {damage} | Giáp: {armor} | Damage thực: {finalDamage} | HP còn: {currentHeath}");

        // =========================
        // CHẾT
        // =========================
        if (currentHeath <= 0)
        {
            currentHeath = 0;
            isDead = true;

            SendMessage("OnBossDeath", SendMessageOptions.DontRequireReceiver);
        }

        targetFill = (float)currentHeath / maxHeath;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(50, "Test");
        }

        if (healthFill != null)
        {
            float current = healthFill.fillAmount;

            healthFill.fillAmount = Mathf.Lerp(
                current,
                targetFill,
                smoothSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (healthUIRoot != null)
        {
            healthUIRoot.gameObject.SetActive(true);
            healthUIRoot.enabled = true;
        }

        if (healthFill != null)
        {
            healthFill.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (healthUIRoot != null)
                healthUIRoot.enabled = false;
            if (healthFill != null)
                healthFill.enabled = false;
        }
    }
    void UpdateHealthUI()
    {
        if (healthFill == null) return;

        float hpPercent = (float)currentHeath / maxHeath;
        healthFill.fillAmount = hpPercent;
    }
}