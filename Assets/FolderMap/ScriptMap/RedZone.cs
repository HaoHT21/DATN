using UnityEngine;
using UnityEngine.UI;

public class RedZone : MonoBehaviour
{
    [Header("Thời gian")]
    public float fillTime = 10f;
    public float drainTime = 3f;

    [Header("UI")]
    public GameObject effectBar;
    public Image effectFill;

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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        playerController = other.GetComponent<PlayerController>();
        effectBar?.SetActive(true);
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
    }
}