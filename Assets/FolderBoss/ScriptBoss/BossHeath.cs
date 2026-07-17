using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class BossHeath : MonoBehaviour
{
    public int currentHeath = 500;
    public int maxHeath = 500;

    public event Action OnDeath;
    [Header("Boss Settings")]
    public bool isFinalBoss = false; // Tích chọn ô này trong Inspector cho con Boss cuối
    [Tooltip("Tên scene Cutscene bạn muốn chạy")]
    public string cutsceneSceneName = "CutScene"; // Nhập tên scene cutscene ở đây


    [Header("UI Health Bar")]
    public Image healthFill;

    [Header("UI Root (Panel chứa thanh máu)")]
    public Image healthUIRoot;

    private bool isDead;

    private float targetFill;
    public float smoothSpeed = 8f;

    [Header("UI")]
    public bool enableHealthUI = true;
    public bool enableManaUI = true;

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
        if (!enableHealthUI)
            return;

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
        currentHeath = maxHeath;
        if (!enableHealthUI)
            return;

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

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHeath -= damage;

        if (currentHeath <= 0)
        {
            currentHeath = 0;
            isDead = true;
            Debug.Log("Boss Death");
            OnDeath?.Invoke(); // THÊM DÒNG NÀY

            SendMessage(
                "OnBossDeath",
                SendMessageOptions.DontRequireReceiver
            );

            // =================================================================
            // CHỈ CẦN CHUYỂN CẢNH SANG CUTSCENE LÀ XONG, KHÔNG CẦN FLOW MANAGER
            // =================================================================
            if (isFinalBoss)
            {
                SceneManager.LoadScene(cutsceneSceneName);
            }
            // =================================================================
        }

        targetFill = (float)currentHeath / maxHeath;
    }

    void Update()
    {
        if (enableHealthUI && healthFill != null)
        {
            healthFill.fillAmount = Mathf.Lerp(
                healthFill.fillAmount,
                targetFill,
                smoothSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enableHealthUI)
            return;
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
        if (!enableHealthUI)
            return;
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