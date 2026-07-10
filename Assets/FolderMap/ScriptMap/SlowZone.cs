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

    [Header("UI")]
    public GameObject effectBar;
    public Image effectFill;

    private PlayerEffect playerEffect;

    private bool playerInside;
    private bool isFilling = true;
    private bool slowActive;

    private float value;

    private void Start()
    {
        if (effectBar != null)
            effectBar.SetActive(false);

        if (effectFill != null)
            effectFill.fillAmount = 0;
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

        if (effectFill != null)
            effectFill.fillAmount = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        playerEffect = other.GetComponent<PlayerEffect>();

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

        if (effectFill != null)
            effectFill.fillAmount = 0;

        if (effectBar != null)
            effectBar.SetActive(false);
    }
}