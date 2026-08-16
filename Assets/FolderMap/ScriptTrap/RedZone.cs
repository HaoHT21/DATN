using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering; // Dùng cho Post-Processing
using UnityEngine.Rendering.Universal; // Dùng cho URP Vignette

public class RedZone : MonoBehaviour
{
    [Header("Thời gian")]
    public float fillTime = 10f;
    public float drainTime = 3f;

    [Header("UI")]
    public GameObject effectBar;
    public Image effectFill;

    [Header("Post Processing Visual")]
    public Volume redEffectVolume; // Kéo Global Volume vào đây
    [Range(0f, 1f)]
    public float maxVignetteIntensity = 0.5f; // Độ đậm tối đa của viền đỏ
    private Vignette redVignette;

    private bool playerInside;
    private bool isFilling = true;
    private bool nauseaActive;

    private float value = 0f;

    private PlayerController playerController;

    void Start()
    {
        if (effectBar != null)
            effectBar.SetActive(false);

        if (effectFill != null)
            effectFill.fillAmount = 0;

        if (redEffectVolume != null)
        {
            redEffectVolume.weight = 0f; // Mặc định ẩn Volume
            if (redEffectVolume.profile != null)
            {
                redEffectVolume.profile = Instantiate(redEffectVolume.profile);
                if (redEffectVolume.profile.TryGet(out redVignette))
                {
                    redVignette.intensity.overrideState = true;
                    redVignette.intensity.value = 0f;
                }
            }
        }
    }

    void Update()
    {
        if (!playerInside)
            return;

        // Thanh tăng
        if (isFilling)
        {
            value += Time.deltaTime / fillTime;

            if (value >= 1f)
            {
                value = 1f;

                if (!nauseaActive)
                {
                    nauseaActive = true;

                    if (playerController != null)
                        playerController.reverseControl = true;
                }

                isFilling = false;
            }
        }
        // Thanh giảm
        else
        {
            value -= Time.deltaTime / drainTime;

            if (value <= 0f)
            {
                value = 0f;

                if (nauseaActive)
                {
                    nauseaActive = false;

                    if (playerController != null)
                        playerController.reverseControl = false;
                }

                isFilling = true;
            }
        }

        if (effectFill != null)
            effectFill.fillAmount = value;
        if (redVignette != null)
        {
            redVignette.intensity.value = value * maxVignetteIntensity;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        playerController = other.GetComponent<PlayerController>();

        if (redEffectVolume != null)
            redEffectVolume.weight = 1f; // Bật Volume RedZone

        if (effectBar != null)
            effectBar.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        nauseaActive = false;
        isFilling = true;
        value = 0f;

        if (playerController != null)
            playerController.reverseControl = false;

        if (effectFill != null)
            effectFill.fillAmount = 0;

        if (effectBar != null)
            effectBar.SetActive(false);

        if (redVignette != null)
            redVignette.intensity.value = 0f;

        if (redEffectVolume != null)
            redEffectVolume.weight = 0f; // Tắt Volume RedZone khi rời đi
    }
}