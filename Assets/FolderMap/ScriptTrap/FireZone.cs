using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering; // Dùng cho Post-Processing
using UnityEngine.Rendering.Universal; // Dùng cho URP Vignette

public class FireZone : MonoBehaviour
{
    [Header("Effect Data")]
    public StatusEffectSO burnEffectSO; // Kéo ScriptableObject Lửa vào đây

    [Header("Fire")]
    [Tooltip("Thời gian chờ (giây) sau khi ngừng nhận nhiệt rồi mới bắt đầu tự động hạ nhiệt")]
    public float delayBeforeCool = 5f;

    [Tooltip("Tổng thời gian (giây) để thanh nhiệt độ giảm hoàn toàn từ 100% về 0%")]
    public float coolTime = 3f;

    [Tooltip("Lượng sát thương Player phải nhận mỗi lần rút máu khi đang bị Bỏng")]
    public int burnDamage = 5;

    [Tooltip("Khoảng thời gian nghỉ (giây) giữa các lần rút máu (ví dụ: 0.3s gây sát thương 1 lần)")]
    public float burnInterval = .3f;

    [Header("UI")]
    [Tooltip("GameObject cha chứa toàn bộ khung thanh UI nhiệt độ (dùng để Ẩn/Hiện)")]
    public GameObject EffectFireBar;

    [Tooltip("Component Image dạng Filled (dùng để làm hiệu ứng tăng/giảm thanh lửa)")]
    public Image EffectFireFill;

    [Header("Post Processing Visual")]
    public Volume globalVolume; // Kéo Global Volume ở Hierarchy vào đây
    [Range(0f, 1f)]
    public float maxVignetteIntensity = 0.5f; // Độ đậm tối đa của viền đỏ

    private Vignette vignetteEffect;
    private EffectManager effectManager;
    private PlayerHealth playerHealth;

    private Coroutine burnRoutine;

    void Start()
    {
        if (EffectFireBar != null)
            EffectFireBar.SetActive(false);

        if (globalVolume != null)
        {
            globalVolume.weight = 0f; // Mặc định ẩn Volume khi chưa vào vùng
            if (globalVolume.profile != null)
            {
                globalVolume.profile = Instantiate(globalVolume.profile);
                if (globalVolume.profile.TryGet(out vignetteEffect))
                {
                    vignetteEffect.intensity.overrideState = true;
                    vignetteEffect.intensity.value = 0f;
                }
            }
        }
    }

    void Update()
    {
        if (effectManager == null)
            return;

        // Tỉ lệ nhiệt độ hiện tại (từ 0.0f -> 1.0f)
        float fireRatio = effectManager.fireValue / effectManager.maxFire;

        // Bật/tắt trạng thái nhận nhiệt trong EffectManager:
        // Nếu đang trong quá trình xả nhiệt/đốt máu (burnRoutine != null) -> Khóa không cho nhận nhiệt
        effectManager.canAddHeat = (burnRoutine == null);

        //--------------------------------
        // Update UI
        //--------------------------------
        if (EffectFireFill != null)
        {
            EffectFireFill.fillAmount = fireRatio;
        }

        //--------------------------------
        // Update Viền Đỏ Màn Hình (Vignette)
        //--------------------------------
        if (vignetteEffect != null)
        {
            // Tăng dần độ đậm viền đỏ dựa theo tỉ lệ nhiệt độ tích tụ
            vignetteEffect.intensity.value = fireRatio * maxVignetteIntensity;
        }

        //--------------------------------
        // Cooldown sau khoảng thời gian delayBeforeCool
        // (Chỉ cooldown tự động nếu chưa kích hoạt BurnRoutine)
        //--------------------------------
        if (burnRoutine == null && Time.time - effectManager.LastFireTime >= delayBeforeCool)
        {
            effectManager.fireValue -= (effectManager.maxFire / coolTime) * Time.deltaTime;
            effectManager.fireValue = Mathf.Clamp(effectManager.fireValue, 0, effectManager.maxFire);
        }

        //--------------------------------
        // Burn kích hoạt Bỏng (KHI ĐẦY THANH LỬA)
        //--------------------------------
        if (effectManager.fireValue >= effectManager.maxFire)
        {
            if (burnRoutine == null)
            {
                if (burnEffectSO != null)
                {
                    // 1. Chỉ áp dụng Status Effect (Visual lửa/Particle) khi bắt đầu quá trình rút thanh
                    effectManager.ApplyEffect(burnEffectSO, coolTime);
                }

                burnRoutine = StartCoroutine(BurnRoutine());
            }
        }
    }

    IEnumerator BurnRoutine()
    {
        // Trong suốt quá trình thanh nhiệt tuột về 0, không nhận thêm bất kỳ nhiệt nào
        while (effectManager.fireValue > 0)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(burnDamage);
            }

            yield return new WaitForSeconds(burnInterval);

            // Giảm nhiệt độ
            effectManager.fireValue -= (effectManager.maxFire / coolTime) * burnInterval;
            effectManager.fireValue = Mathf.Clamp(effectManager.fireValue, 0, effectManager.maxFire);
        }

        StopBurnEffect();
        burnRoutine = null;

        // Mở lại khả năng nhận nhiệt sau khi đã tuột hết về 0
        if (effectManager != null)
        {
            effectManager.canAddHeat = true;
        }
    }

    private void StopBurnEffect()
    {
        if (effectManager != null && burnEffectSO != null)
        {
            effectManager.RemoveEffectBySO(burnEffectSO);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        effectManager = other.GetComponent<EffectManager>();
        playerHealth = other.GetComponent<PlayerHealth>();

        if (globalVolume != null)
            globalVolume.weight = 1f; // Bật Volume vùng cháy

        if (EffectFireBar != null)
            EffectFireBar.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        // Xóa Effect Lửa ngay lập tức nếu thoát ra ngoài
        StopBurnEffect();

        if (effectManager != null)
        {
            effectManager.canAddHeat = true; // Mở lại quyền nhận nhiệt
            effectManager.ResetFireHeat();
        }

        if (EffectFireFill != null)
            EffectFireFill.fillAmount = 0;

        if (EffectFireBar != null)
            EffectFireBar.SetActive(false);

        if (vignetteEffect != null)
            vignetteEffect.intensity.value = 0f;

        if (globalVolume != null)
            globalVolume.weight = 0f; // Tắt Volume vùng cháy khi rời đi

        effectManager = null;
        playerHealth = null;
    }
}