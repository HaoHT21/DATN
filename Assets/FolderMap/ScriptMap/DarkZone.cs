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
        if (!playerInside)
            return;

        // Thanh tăng
        if (isFilling)
        {
            value += Time.deltaTime / fillTime;

            if (value >= 1f)
            {
                value = 1f;

                if (!darkActive)
                {
                    darkActive = true;
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

                ResetLight();

                isFilling = true;
            }
        }

        if (effectFill != null)
            effectFill.fillAmount = value;

        float targetGlobal =
    darkActive ? globalDarkIntensity : globalNormalIntensity;

        if (globalLight != null)
        {
            globalLight.intensity =
                Mathf.MoveTowards(
                    globalLight.intensity,
                    targetGlobal,
                    lightChangeSpeed * Time.deltaTime);
        }

        float targetPlayer =
            darkActive ? 1f : 0f;

        if (playerSpotLight != null)
        {
            playerSpotLight.intensity =
                Mathf.MoveTowards(
                    playerSpotLight.intensity,
                    targetPlayer,
                    lightChangeSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger: " + other.name);

        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Player Enter");

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

        playerInside = false;

        value = 0;
        isFilling = true;

        ResetLight();

        if (effectFill != null)
            effectFill.fillAmount = 0;

        if (effectBar != null)
            effectBar.SetActive(false);
    }

    private void ResetLight()
    {
        darkActive = false;
    }
}