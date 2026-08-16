using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class IceZone : MonoBehaviour
{
    [Header("Thời gian tích & xả")]
    public float fillTime = 10f;
    public float drainTime = 3f;

    [Header("Thời gian Chu kỳ Bão tuyết")]
    public float snowDuration = 5f;   // Thời gian tuyết rơi đầy đủ
    public float pauseDuration = 3f;  // Thời gian tạm nghỉ
    public float fadeDuration = 1.5f; // Tốc độ hiện/ẩn mượt

    [Header("Particle System & UI")]
    public ParticleSystem snowParticle;
    public GameObject EffectIceBar;
    public Image EffectIceFill;

    [Header("Post Processing Visual")]
    public Volume freezeEffectVolume;
    [Range(0f, 1f)]
    public float maxFreezeIntensity = 0.5f;
    private Vignette freezeVignette;

    [Header("Effect Data")]
    public StatusEffectSO freezeEffectSO;

    private EffectManager playerEffect;

    private bool playerInside = false;
    private bool freezeActive = false;
    private bool isSnowing = false;

    private float value = 0f;
    private float stormTimer = 0f;

    private Coroutine fadeCoroutine;

    void Start()
    {
        if (EffectIceBar != null)
            EffectIceBar.SetActive(false);

        if (EffectIceFill != null)
            EffectIceFill.fillAmount = 0;

        if (snowParticle != null && !snowParticle.isPlaying)
        {
            snowParticle.Play();
        }

        if (freezeEffectVolume != null)
        {
            freezeEffectVolume.weight = 0f; // Mặc định ẩn Volume
            if (freezeEffectVolume.profile != null)
            {
                freezeEffectVolume.profile = Instantiate(freezeEffectVolume.profile);
                if (freezeEffectVolume.profile.TryGet(out freezeVignette))
                {
                    freezeVignette.intensity.overrideState = true;
                    freezeVignette.intensity.value = 0f;
                }
            }
        }

        // Mặc định bắt đầu trạng thái ẩn
        isSnowing = false;
        SetAlphaImmediate(0f);
    }

    void Update()
    {
        HandleSnowstormCycle();
        HandleIceLogic();
        UpdateUI();
    }

    private void HandleSnowstormCycle()
    {
        stormTimer += Time.deltaTime;

        if (isSnowing && stormTimer >= snowDuration)
        {
            HideSnowstorm();
        }
        else if (!isSnowing && stormTimer >= pauseDuration)
        {
            ShowSnowstorm();
        }
    }

    private void ShowSnowstorm()
    {
        isSnowing = true;
        stormTimer = 0f;
        StartFade(1f);
    }

    private void HideSnowstorm()
    {
        isSnowing = false;
        stormTimer = 0f;
        StartFade(0f);
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = GetCurrentAlpha();
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            ApplyAlphaToParticleLifetime(currentAlpha);
            yield return null;
        }

        ApplyAlphaToParticleLifetime(targetAlpha);
        fadeCoroutine = null;
    }

    /// <summary>
    /// Thay đổi Gradient Alpha của Color Over Lifetime (Mode: Blend)
    /// Làm hạt tuyết mượt mà ẩn/hiện toàn bộ
    /// </summary>
    private void ApplyAlphaToParticleLifetime(float targetAlpha)
    {
        if (snowParticle == null) return;

        var colorOverLifetime = snowParticle.colorOverLifetime;
        colorOverLifetime.enabled = true;

        // Tạo Gradient Blend từ Alpha mong muốn về 0 ở cuối đời hạt
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(targetAlpha, 0.0f), new GradientAlphaKey(targetAlpha * 0.5f, 0.8f), new GradientAlphaKey(0f, 1.0f) }
        );

        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);
    }

    private void SetAlphaImmediate(float alpha)
    {
        ApplyAlphaToParticleLifetime(alpha);
    }

    private float GetCurrentAlpha()
    {
        return isSnowing ? 0f : 1f;
    }

    private void HandleIceLogic()
    {
        if (!playerInside) return;

        if (value >= 1f || (freezeActive && value > 0f))
        {
            value -= Time.deltaTime / drainTime;

            if (value <= 0f)
            {
                value = 0f;
                UnfreezePlayer();
            }
        }
        else if (isSnowing)
        {
            value += Time.deltaTime / fillTime;

            if (value >= 1f)
            {
                value = 1f;
                FreezePlayer();
            }
        }
    }

    /// <summary>
    /// Hàm public cho phép HeaterTrigger gọi sang để làm giảm độ đóng băng
    /// </summary>
    public void ReduceFreezeValue(float amount)
    {
        if (value > 0f)
        {
            value -= amount;
            if (value <= 0f)
            {
                value = 0f;
                UnfreezePlayer();
            }
        }
    }

    private void FreezePlayer()
    {
        if (!freezeActive && playerEffect != null)
        {
            freezeActive = true;
            playerEffect.ApplyEffect(freezeEffectSO, drainTime);
        }
    }

    private void UnfreezePlayer()
    {
        if (freezeActive && playerEffect != null)
        {
            freezeActive = false;
            playerEffect.RemoveEffectBySO(freezeEffectSO);
        }
    }

    private void UpdateUI()
    {
        if (EffectIceFill != null && playerInside)
        {
            EffectIceFill.fillAmount = value;
        }

        if (freezeVignette != null)
        {
            freezeVignette.intensity.value = value * maxFreezeIntensity;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            playerEffect = other.GetComponent<EffectManager>();

            if (freezeEffectVolume != null)
                freezeEffectVolume.weight = 1f; // Bật Volume vùng băng

            if (EffectIceBar != null)
                EffectIceBar.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            UnfreezePlayer();
            value = 0f;

            if (EffectIceFill != null)
                EffectIceFill.fillAmount = 0f;

            if (EffectIceBar != null)
                EffectIceBar.SetActive(false);

            if (freezeVignette != null)
                freezeVignette.intensity.value = 0f;

            if (freezeEffectVolume != null)
                freezeEffectVolume.weight = 0f; // Tắt Volume vùng băng khi rời đi
        }
    }
}