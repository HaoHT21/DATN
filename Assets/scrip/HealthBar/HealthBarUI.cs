using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// View thanh máu: Slider hoặc Image Fill, text tùy chọn, Lerp + nhấp nháy khi trúng đòn.
/// Chỉ cập nhật khi nhận SetHealth / ApplyChange — không poll IHealthProvider mỗi frame.
/// </summary>
[DisallowMultipleComponent]
public class HealthBarUI : MonoBehaviour
{
    [Header("Hiển thị")]
    [SerializeField] private HealthBarFillMode fillMode = HealthBarFillMode.Slider;
    [SerializeField] private HealthBarTextMode textMode = HealthBarTextMode.CurrentOverMax;

    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image delayFillImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Text legacyHealthText;

    [Header("Hiệu ứng")]
    [SerializeField] private HealthBarSmoothSettings smooth = new HealthBarSmoothSettings();
    [SerializeField] private HealthBarBlinkSettings blink = new HealthBarBlinkSettings();
    [SerializeField] private Graphic blinkTarget;

    private float _displayNormalized = 1f;
    private float _targetNormalized = 1f;
    private float _delayNormalized = 1f;
    private bool _isAnimating;
    private Color _blinkOriginalColor;
    private Coroutine _blinkRoutine;

    private void Awake()
    {
        if (blinkTarget == null)
        {
            if (fillImage != null) blinkTarget = fillImage;
            else if (slider != null && slider.fillRect != null)
                blinkTarget = slider.fillRect.GetComponent<Image>();
        }

        if (blinkTarget != null)
            _blinkOriginalColor = blinkTarget.color;

        ApplyFillInstant(1f);
        RefreshText(100, 100);
    }

    /// <summary>Đồng bộ ngay lập tức (spawn, respawn).</summary>
    public void SetHealth(int current, int max, bool playDamageEffects = false)
    {
        max = Mathf.Max(1, max);
        current = Mathf.Clamp(current, 0, max);
        float normalized = (float)current / max;

        _targetNormalized = normalized;
        _delayNormalized = normalized;

        if (!smooth.enabled)
        {
            _displayNormalized = normalized;
            ApplyFillInstant(normalized);
        }
        else
        {
            _displayNormalized = normalized;
            ApplyFillInstant(normalized);
            _isAnimating = false;
        }

        RefreshText(current, max);

        if (playDamageEffects && blink.enabled)
            PlayDamageBlink();
    }

    /// <summary>Cập nhật khi máu thay đổi từ sự kiện.</summary>
    public void ApplyChange(HealthChangeInfo info)
    {
        int max = Mathf.Max(1, info.Max);
        int current = Mathf.Clamp(info.Current, 0, max);
        _targetNormalized = (float)current / max;

        RefreshText(current, max);

        if (info.WasDamage && blink.enabled)
            PlayDamageBlink();

        if (!smooth.enabled)
        {
            _displayNormalized = _targetNormalized;
            _delayNormalized = _targetNormalized;
            ApplyFillInstant(_displayNormalized);
            return;
        }

        if (info.WasHeal)
        {
            _displayNormalized = _targetNormalized;
            _delayNormalized = _targetNormalized;
            ApplyFillInstant(_displayNormalized);
            _isAnimating = false;
            return;
        }

        if (info.WasDamage && smooth.useDelayBar && delayFillImage != null)
            _delayNormalized = Mathf.Max(_delayNormalized, _displayNormalized);
        else
            _delayNormalized = _targetNormalized;

        _isAnimating = true;
    }

    private void Update()
    {
        if (!smooth.enabled || !_isAnimating)
            return;

        _displayNormalized = Mathf.Lerp(_displayNormalized, _targetNormalized, smooth.smoothSpeed * Time.deltaTime);

        if (smooth.useDelayBar && delayFillImage != null)
            _delayNormalized = Mathf.Lerp(_delayNormalized, _targetNormalized, smooth.smoothSpeed * 0.5f * Time.deltaTime);
        else
            _delayNormalized = _displayNormalized;

        ApplyFillInstant(_displayNormalized);
        if (smooth.useDelayBar && delayFillImage != null)
            SetImageFill(delayFillImage, _delayNormalized);

        if (Mathf.Approximately(_displayNormalized, _targetNormalized)
            && Mathf.Approximately(_delayNormalized, _targetNormalized))
        {
            _isAnimating = false;
        }
    }

    private void ApplyFillInstant(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        switch (fillMode)
        {
            case HealthBarFillMode.Slider:
                if (slider != null)
                {
                    slider.minValue = 0f;
                    slider.maxValue = 1f;
                    slider.value = normalized;
                }
                break;

            case HealthBarFillMode.ImageFill:
                if (fillImage != null)
                    SetImageFill(fillImage, normalized);
                break;
        }
    }

    private static void SetImageFill(Image image, float normalized)
    {
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillAmount = normalized;
    }

    private void RefreshText(int current, int max)
    {
        if (textMode == HealthBarTextMode.None)
            return;

        string label = $"{current} / {max}";

        if (healthText != null)
            healthText.text = label;
        if (legacyHealthText != null)
            legacyHealthText.text = label;
    }

    private void PlayDamageBlink()
    {
        if (blinkTarget == null)
            return;

        if (_blinkRoutine != null)
            StopCoroutine(_blinkRoutine);

        _blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        for (int i = 0; i < blink.flashCount; i++)
        {
            blinkTarget.color = blink.damageFlashColor;
            yield return new WaitForSeconds(blink.flashInterval);
            blinkTarget.color = _blinkOriginalColor;
            yield return new WaitForSeconds(blink.flashInterval);
        }

        blinkTarget.color = _blinkOriginalColor;
        _blinkRoutine = null;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
