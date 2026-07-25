using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering; // Dùng cho Post-Processing
using UnityEngine.Rendering.Universal; // Dùng cho URP Vignette

public class FireZone : MonoBehaviour
{
    [Header("Fire")]
    public float delayBeforeCool = 5f;
    public float coolTime = 3f;

    public int burnDamage = 5;
    public float burnInterval = .3f;

    [Header("UI")]
    public GameObject EffectFireBar;
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

        // Lấy Component Vignette từ Global Volume
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignetteEffect);
        }
    }

    void Update()
    {
        if (effectManager == null)
            return;

        // Tỉ lệ nhiệt độ hiện tại (từ 0.0f -> 1.0f)
        float fireRatio = effectManager.fireValue / effectManager.maxFire;

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
        // Cooldown sau 5 giây
        //--------------------------------
        if (Time.time - effectManager.LastFireTime >= delayBeforeCool)
        {
            effectManager.fireValue -=
                effectManager.maxFire /
                coolTime *
                Time.deltaTime;

            effectManager.fireValue =
                Mathf.Clamp(
                    effectManager.fireValue,
                    0,
                    effectManager.maxFire);
        }

        //--------------------------------
        // Burn
        //--------------------------------
        if (effectManager.fireValue >= effectManager.maxFire)
        {
            if (burnRoutine == null)
            {
                burnRoutine =
                    StartCoroutine(BurnRoutine());
            }
        }
    }

    IEnumerator BurnRoutine()
    {
        while (effectManager.fireValue > 0)
        {
            playerHealth.TakeDamage(burnDamage);

            yield return new WaitForSeconds(burnInterval);

            effectManager.fireValue -=
                effectManager.maxFire /
                coolTime *
                burnInterval;

            effectManager.fireValue =
                Mathf.Clamp(
                    effectManager.fireValue,
                    0,
                    effectManager.maxFire);
        }

        burnRoutine = null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        effectManager =
            other.GetComponent<EffectManager>();

        playerHealth =
            other.GetComponent<PlayerHealth>();

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

        if (effectManager != null)
            effectManager.ResetFireHeat();

        if (EffectFireFill != null)
            EffectFireFill.fillAmount = 0;

        if (EffectFireBar != null)
            EffectFireBar.SetActive(false);

        // Reset viền đỏ về 0 khi thoát vùng cháy
        if (vignetteEffect != null)
        {
            vignetteEffect.intensity.value = 0f;
        }

        effectManager = null;
        playerHealth = null;
    }
}