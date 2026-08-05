using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class DarkZone : MonoBehaviour
{
    [Header("Thời gian")]
    public float fillTime = 10f;
    public float drainTime = 3f;

    [Header("Ánh sáng")]
    public Light2D globalLight;
    public float globalNormalIntensity = 1f;
    public float globalDarkIntensity = 0.2f;

    [Header("UI")]
    public GameObject effectBar;
    public Image effectFill;

    [Header("Smooth")]
    public float lightChangeSpeed = 2f;

    private Light2D playerSpotLight;

    private bool playerInside;
    private bool isFilling = true;
    private bool darkActive;

    private float value;

    private void Start()
    {
        if (effectBar != null)
            effectBar.SetActive(false);

        if (effectFill != null)
            effectFill.fillAmount = 0;

        if (globalLight != null)
            globalLight.intensity = globalNormalIntensity;
    }

    private void Update()
    {
        // 1. Quản lý trạng thái thanh đếm
        if (playerInside)
        {
            // Nếu Player bị Destroy đột ngột khi đang trong zone
            if (playerSpotLight == null)
            {
                OnPlayerLeftOrDied();
            }
            else
            {
                HandleZoneTimer();
            }
        }

        // 2. Cập nhật ánh sáng (Luôn chạy để làm mượt đèn Global và đèn Player về 0)
        UpdateLighting();
    }

    private void HandleZoneTimer()
    {
        // Thanh tăng
        if (isFilling)
        {
            value += Time.deltaTime / fillTime;

            if (value >= 1f)
            {
                value = 1f;
                darkActive = true;
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
                ResetLight();
                isFilling = true;
            }
        }

        if (effectFill != null)
            effectFill.fillAmount = value;
    }

    private void UpdateLighting()
    {
        // Global Light transition
        float targetGlobal = darkActive ? globalDarkIntensity : globalNormalIntensity;

        if (globalLight != null)
        {
            globalLight.intensity = Mathf.MoveTowards(
                globalLight.intensity,
                targetGlobal,
                lightChangeSpeed * Time.deltaTime);
        }

        // Player Light transition
        if (playerSpotLight != null)
        {
            // Chỉ bật sáng (target = 1) khi Player ở TRONG vùng VÀ darkActive = true
            // Nếu Player rời đi (playerInside = false), target sẽ là 0
            float targetPlayer = (playerInside && darkActive) ? 1f : 0f;

            playerSpotLight.intensity = Mathf.MoveTowards(
                playerSpotLight.intensity,
                targetPlayer,
                lightChangeSpeed * Time.deltaTime);

            // Khi Player đã rời đi và đèn đã tắt hoàn toàn về 0 -> Mới giải phóng tham chiếu
            if (!playerInside && Mathf.Approximately(playerSpotLight.intensity, 0f))
            {
                playerSpotLight = null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        playerSpotLight = other.GetComponentInChildren<Light2D>();

        if (playerSpotLight != null)
            playerSpotLight.intensity = 0f;

        if (effectBar != null)
            effectBar.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        OnPlayerLeftOrDied();
    }

    private void OnDisable()
    {
        OnPlayerLeftOrDied();
        // Force reset khẩn cấp nếu GameObject/Scene bị Disable
        playerSpotLight = null;
    }

    private void OnPlayerLeftOrDied()
    {
        playerInside = false;
        value = 0f;
        isFilling = true;

        ResetLight();

        if (effectFill != null)
            effectFill.fillAmount = 0f;

        if (effectBar != null)
            effectBar.SetActive(false);

        // Lưu ý: Không set playerSpotLight = null ở đây ngay 
        // để UpdateLighting() kịp kéo intensity của đèn Player về 0.
    }

    private void ResetLight()
    {
        darkActive = false;
    }
}