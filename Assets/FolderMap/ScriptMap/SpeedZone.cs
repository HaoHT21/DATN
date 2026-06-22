using UnityEngine;
using UnityEngine.UI;

public class SpeedZone : MonoBehaviour
{
    [Header("Buff")]
    public float speedBoost = 3f;

    [Header("Thời gian")]
    public float fillTime = 10f;   // thời gian tăng đầy
    public float drainTime = 3f;   // thời gian giảm hết

    [Header("UI")]
    public GameObject EffectWindBar;
    public Image EffectWindFill;

    private PlayerEffect playerEffect;

    private bool playerInside;
    private bool isFilling = true;
    private bool buffActive = false;

    private float value = 0f;

    void Start()
    {
        if (EffectWindBar != null)
            EffectWindBar.SetActive(false);

        if (EffectWindFill != null)
            EffectWindFill.fillAmount = 0;
    }

    void Update()
    {
        if (!playerInside)
            return;

        // Giai đoạn tăng
        if (isFilling)
        {
            value += Time.deltaTime / fillTime;

            if (value >= 1f)
            {
                value = 1f;

                // nhận buff
                if (!buffActive)
                {
                    buffActive = true;

                    if (playerEffect != null)
                        playerEffect.AddSpeed(speedBoost);
                }

                // chuyển sang giảm
                isFilling = false;
            }
        }

        // Giai đoạn giảm
        else
        {
            value -= Time.deltaTime / drainTime;

            if (value <= 0f)
            {
                value = 0f;

                // mất buff
                if (buffActive)
                {
                    buffActive = false;

                    if (playerEffect != null)
                        playerEffect.RemoveSpeed(speedBoost);
                }

                // quay lại tăng
                isFilling = true;
            }
        }

        // cập nhật UI
        if (EffectWindFill != null)
            EffectWindFill.fillAmount = value;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            playerEffect =
                other.GetComponent<PlayerEffect>();

            if (EffectWindBar != null)
                EffectWindBar.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            // nếu đang buff thì bỏ buff
            if (buffActive && playerEffect != null)
            {
                playerEffect.RemoveSpeed(speedBoost);
            }

            buffActive = false;
            isFilling = true;
            value = 0;

            if (EffectWindFill != null)
                EffectWindFill.fillAmount = 0;

            if (EffectWindBar != null)
                EffectWindBar.SetActive(false);
        }
    }
}