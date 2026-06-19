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

    private PlayerEffect playerEffect;

    private bool playerInside;
    private bool isFilling = true;
    private bool freezeActive = false;

    private float value = 0f;

    // lưu tốc độ cũ
    private float oldSpeed;

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

                    oldSpeed =
                        playerEffect.currentMoveSpeed;

                    playerEffect.currentMoveSpeed = 0;
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

                    playerEffect.currentMoveSpeed =
                        oldSpeed;
                }

                isFilling = true;
            }
        }

        // áp dụng tốc độ
        if (playerEffect != null)
        {
            playerEffect.SendMessage(
                "ApplyStats",
                SendMessageOptions.DontRequireReceiver
            );
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
                other.GetComponent<PlayerEffect>();

            if (EffectIceBar != null)
                EffectIceBar.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            // trả tốc nếu đang đóng băng
            if (freezeActive && playerEffect != null)
            {
                playerEffect.currentMoveSpeed =
                    oldSpeed;

                playerEffect.SendMessage(
                    "ApplyStats",
                    SendMessageOptions.DontRequireReceiver
                );
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