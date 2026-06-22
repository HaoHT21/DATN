using UnityEngine;
using UnityEngine.UI;

public class FireZone : MonoBehaviour
{
    [Header("Sát thương")]
    public int fireDamage = 10;

    [Header("Thời gian")]
    public float fillTime = 10f;   // thời gian tăng đầy
    public float drainTime = 3f;   // thời gian giảm hết

    [Header("UI")]
    public GameObject EffectFireBar;
    public Image EffectFireFill;

    private PlayerHealth playerHealth;

    private bool playerInside;
    private bool isFilling = true;
    private bool burnActive = false;

    private float value = 0f;

    void Start()
    {
        if (EffectFireBar != null)
            EffectFireBar.SetActive(false);

        if (EffectFireFill != null)
            EffectFireFill.fillAmount = 0;
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

                // gây sát thương khi đầy
                if (!burnActive)
                {
                    burnActive = true;

                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(
                            fireDamage
                        );
                    }
                }

                // chuyển sang giảm
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

                // kết thúc cháy
                burnActive = false;

                // quay lại tăng
                isFilling = true;
            }
        }

        // cập nhật UI
        if (EffectFireFill != null)
        {
            EffectFireFill.fillAmount = value;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            playerHealth =
                other.GetComponent<PlayerHealth>();

            if (EffectFireBar != null)
                EffectFireBar.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            burnActive = false;
            isFilling = true;
            value = 0;

            if (EffectFireFill != null)
                EffectFireFill.fillAmount = 0;

            if (EffectFireBar != null)
                EffectFireBar.SetActive(false);
        }
    }
}