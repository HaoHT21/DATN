using UnityEngine;
using UnityEngine.UI;

public class SlowZone : MonoBehaviour
{

    [Header("Thời gian Chờ Ngẫu Nhiên (Random Delay)")]
    [Tooltip("Thời gian chờ tối thiểu trước khi bắt đầu chạy thanh")]
    public float minDelay = 1f;
    [Tooltip("Thời gian chờ tối đa trước khi bắt đầu chạy thanh")]
    public float maxDelay = 10f;

    [Header("Thời gian Tích/Xả")]
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
    private bool slowActive;

    private float value;
    private float currentWaitTimer;
    private ZoneState currentState = ZoneState.Waiting;

    // Giới hạn Alpha tối đa: 220 / 255 ≈ 0.86f
    private const float MAX_ALPHA = 220f / 255f;

    private enum ZoneState
    {
        Waiting, // Đang chờ random time trước khi nạp
        Filling, // Đang tích lũy thanh nhiệt/chậm
        Draining // Đang xả thanh và áp dụng làm chậm
    }

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

        switch (currentState)
        {
            case ZoneState.Waiting:
                // Đếm ngược thời gian chờ ngẫu nhiên
                currentWaitTimer -= Time.deltaTime;
                value = 0f;

                if (currentWaitTimer <= 0f)
                {
                    // Hết thời gian chờ -> Chuyển sang trạng thái Nạp (Filling)
                    currentState = ZoneState.Filling;
                }
                break;

            case ZoneState.Filling:
                // Tăng thanh
                value += Time.deltaTime / fillTime;

                if (value >= 1f)
                {
                    value = 1f;

                    // Kích hoạt hiệu ứng làm chậm khi đầy thanh
                    if (!slowActive && playerEffect != null)
                    {
                        slowActive = true;
                        playerEffect.SetSlow(slowPercent);
                    }

                    // Đầy thanh -> Chuyển sang trạng thái Rút (Draining)
                    currentState = ZoneState.Draining;
                }
                break;

            case ZoneState.Draining:
                // Giảm thanh
                value -= Time.deltaTime / drainTime;

                if (value <= 0f)
                {
                    value = 0f;

                    // Gỡ bỏ làm chậm khi hết thanh
                    if (slowActive && playerEffect != null)
                    {
                        slowActive = false;
                        playerEffect.RemoveSlow();
                    }

                    // Hết thanh -> Random thời gian chờ mới cho chu kỳ tiếp theo
                    StartNewWaitTimer();
                }
                break;
        }

        // Cập nhật UI
        if (effectFill != null)
            effectFill.fillAmount = value;

        // Cập nhật Alpha của bão cát dựa theo value
        SetSandstormAlpha(value * MAX_ALPHA);
    }

    private void StartNewWaitTimer()
    {
        currentState = ZoneState.Waiting;
        currentWaitTimer = Random.Range(minDelay, maxDelay);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        playerEffect = other.GetComponent<EffectManager>();

        if (effectBar != null)
            effectBar.SetActive(true);

        // Bắt đầu chu kỳ với một khoảng thời gian chờ ngẫu nhiên
        StartNewWaitTimer();
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
        currentState = ZoneState.Waiting;
        value = 0f;

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