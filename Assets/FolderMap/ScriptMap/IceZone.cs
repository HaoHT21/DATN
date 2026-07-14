using UnityEngine;
using UnityEngine.UI;

public class IceZone : MonoBehaviour
{
    [Header("Thời gian")]
    public float fillTime = 10f;
    public float drainTime = 3f;

    [Header("UI")]
    public GameObject EffectIceBar;
    public Image EffectIceFill;

    private EffectManager playerEffect;

    private bool playerInside;
    private bool isFilling = true;
    private bool freezeActive = false;

    private float value = 0f;

    void Start()
    {
        if (EffectIceBar != null)
            EffectIceBar.SetActive(false);

        if (EffectIceFill != null)
            EffectIceFill.fillAmount = 0;
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

                // đóng băng
                if (!freezeActive && playerEffect != null)
                {
                    freezeActive = true;

                    playerEffect.AddFreeze();
                }

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

                // bỏ đóng băng
                if (freezeActive && playerEffect != null)
                {
                    freezeActive = false;

                    playerEffect.RemoveFreeze();
                }

                isFilling = true;
            }
        }

        // cập nhật UI
        if (EffectIceFill != null)
            EffectIceFill.fillAmount = value;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            playerEffect =
                other.GetComponent<EffectManager>();

            if (EffectIceBar != null)
                EffectIceBar.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (freezeActive && playerEffect != null)
            {
                freezeActive = false;

                playerEffect.RemoveFreeze();
            }

            freezeActive = false;
            isFilling = true;
            value = 0;

            if (EffectIceFill != null)
                EffectIceFill.fillAmount = 0;

            if (EffectIceBar != null)
                EffectIceBar.SetActive(false);
        }
    }
}