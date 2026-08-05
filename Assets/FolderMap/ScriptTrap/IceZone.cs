using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Dùng cho URP Vignette

public class IceZone : MonoBehaviour
{
    [Header("Thời gian tích & xả")]
    public float fillTime = 10f;
    public float drainTime = 3f;

    [Header("Thời gian Chu kỳ Bão tuyết")]
    public float snowDuration = 5f;   // Thời gian tuyết rơi đầy đủ
    public float pauseDuration = 3f;  // Thời gian tạm nghỉ
    public float fadeDuration = 1.5f; // Tăng lên 1.5s để thấy rõ hiệu ứng hiện/ẩn

    [Header("Particle System & UI")]
    public ParticleSystem snowParticle;
    public GameObject EffectIceBar;
    public Image EffectIceFill;

    // Thêm trường khai báo SO ở đầu script IceZone
    [Header("Post Processing Visual")]
    public Volume freezeEffectVolume; // Kéo Global Volume vào đây
    [Range(0f, 1f)]
    public float maxFreezeIntensity = 0.5f;  //Độ đậm tối đa của viền xanh
    private Vignette freezeVignette;

    [Header("Effect Data")]
    public StatusEffectSO freezeEffectSO;

    private EffectManager playerEffect;

    private bool playerInside = false;
    private bool freezeActive = false;
    private bool isSnowing = false;

    private float value = 0f;
    private float stormTimer = 0f;

    private ParticleSystemRenderer particleRenderer;
    private Material particleMaterial;
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (EffectIceBar != null)
            EffectIceBar.SetActive(false);

        if (EffectIceFill != null)
            EffectIceFill.fillAmount = 0;

        if (snowParticle != null)
        {
            // Lấy Renderer và tạo bản sao Material riêng cho Zone này
            particleRenderer = snowParticle.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
            {
                particleMaterial = particleRenderer.material; // Auto instance material
            }

            if (!snowParticle.isPlaying)
            {
                snowParticle.Play();
            }
        }
        // Lấy Vignette từ Global Volume
        if (freezeEffectVolume != null && freezeEffectVolume.profile != null)
        {
            freezeEffectVolume.profile.TryGet(out freezeVignette);

            if (freezeVignette != null)
            {
                freezeVignette.intensity.value = 0f;
            }
        }
        // Đặt mặc định bắt đầu ở trạng thái ẨN
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

        // Chỉ cần đợi đúng snowDuration là bắt đầu ẩn
        if (isSnowing && stormTimer >= snowDuration)
        {
            HideSnowstorm();
        }
        // Chỉ cần đợi đúng pauseDuration là bắt đầu hiện
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
            ApplyAlpha(currentAlpha);
            yield return null;
        }

        ApplyAlpha(targetAlpha);
        fadeCoroutine = null;
    }

    // Đổi Alpha trực tiếp qua Material của Particle để có hiệu lực lên TOÀN BỘ hạt đang rơi
    private void ApplyAlpha(float alpha)
    {
        if (particleMaterial != null)
        {
            // Thay đổi tint color của Material (thường là _Color)
            if (particleMaterial.HasProperty("_Color"))
            {
                Color c = particleMaterial.color;
                c.a = alpha;
                particleMaterial.color = c;
            }
            // Hỗ trợ Shaders của Universal Render Pipeline (URP)
            else if (particleMaterial.HasProperty("_BaseColor"))
            {
                Color c = particleMaterial.GetColor("_BaseColor");
                c.a = alpha;
                particleMaterial.SetColor("_BaseColor", c);
            }
        }

        // Đồng thời cập nhật MainModule startColor cho chắc chắn
        if (snowParticle != null)
        {
            var main = snowParticle.main;
            Color c = main.startColor.color;
            c.a = alpha;
            main.startColor = c;
        }
    }

    private void SetAlphaImmediate(float alpha)
    {
        ApplyAlpha(alpha);
    }

    private float GetCurrentAlpha()
    {
        if (particleMaterial != null)
        {
            if (particleMaterial.HasProperty("_Color"))
                return particleMaterial.color.a;
            if (particleMaterial.HasProperty("_BaseColor"))
                return particleMaterial.GetColor("_BaseColor").a;
        }
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

    private void FreezePlayer()
    {
        if (!freezeActive && playerEffect != null)
        {
            freezeActive = true;
            playerEffect.ApplyEffect(freezeEffectSO, drainTime); // Sử dụng drainTime làm thời gian đóng băng
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

        // Hiệu ứng màn hình theo thanh đóng băng
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

            // Reset viền xanh về 0 khi thoát vùng băng
            if (freezeVignette != null)
            {
                freezeVignette.intensity.value = 0f;
            }
        }
    }
}