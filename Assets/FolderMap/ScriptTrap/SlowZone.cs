using UnityEngine;
using UnityEngine.UI;

public class SlowZone : MonoBehaviour
{
    [Header("Thời gian")]
    public float fillTime = 5f;
    public float drainTime = 2f;

    [Header("Làm chậm")]
    [Range(0f, 1f)]
    public float slowPercent = 0.5f; // 0.5 = giảm 50%

    [Header("Visual Sandstorm Shader")]
    public GameObject sandstormObject; // Kéo GameObject chứa SpriteRenderer bão cát vào đây

    [Header("UI")]
    public GameObject effectBar;
    public Image effectFill;

    private EffectManager playerEffect;
    private SpriteRenderer sandstormSR;

    private bool playerInside;
    private bool isFilling = true;
    private bool slowActive;

    private float value;

    // Giới hạn Alpha tối đa: 150 / 255 ≈ 0.588f
    private const float MAX_ALPHA = 220f / 255f;

    private void Start()
    {
        if (effectBar != null)
            effectBar.SetActive(false);

        if (effectFill != null)
            effectFill.fillAmount = 0;

        // Lấy SpriteRenderer từ sandstormObject
        if (sandstormObject != null)
        {
            sandstormSR = sandstormObject.GetComponent<SpriteRenderer>();

            // Đảm bảo ban đầu Alpha = 0 (ẩn bão cát)
            SetSandstormAlpha(0f);
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        // Tăng thanh
        if (isFilling)
        {
            value += Time.deltaTime / fillTime;

            if (value >= 1f)
            {
                value = 1f;

                if (!slowActive && playerEffect != null)
                {
                    slowActive = true;
                    playerEffect.SetSlow(slowPercent);
                }

                isFilling = false;
            }
        }
        // Giảm thanh
        else
        {
            value -= Time.deltaTime / drainTime;

            if (value <= 0)
            {
                value = 0;

                if (slowActive && playerEffect != null)
                {
                    slowActive = false;
                    playerEffect.RemoveSlow();
                }

                isFilling = true;
            }
        }

        // Cập nhật UI
        if (effectFill != null)
            effectFill.fillAmount = value;

        // Cập nhật Alpha của bão cát dựa theo value (từ 0 -> 150/255)
        SetSandstormAlpha(value * MAX_ALPHA);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        playerEffect = other.GetComponent<EffectManager>();

        if (effectBar != null)
            effectBar.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (slowActive && playerEffect != null)
        {
            playerEffect.RemoveSlow();
        }

        slowActive = false;
        isFilling = true;
        value = 0;

        // Cập nhật UI và reset Alpha về 0 ngay khi thoát
        if (effectFill != null)
            effectFill.fillAmount = 0;

        if (effectBar != null)
            effectBar.SetActive(false);

        SetSandstormAlpha(0f);
    }

    // Hàm phụ trợ để gán Alpha cho SpriteRenderer mượt mà
    private void SetSandstormAlpha(float targetAlpha)
    {
        if (sandstormSR != null)
        {
            Color currentColor = sandstormSR.color;
            currentColor.a = targetAlpha;
            sandstormSR.color = currentColor;
        }
    }
}