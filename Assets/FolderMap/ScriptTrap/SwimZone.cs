using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering; // Dùng cho Post-Processing
using UnityEngine.Rendering.Universal; // Dùng cho URP Vignette

public class SwimZone : MonoBehaviour
{
    [Header("Time")]
    public float fillTime = 8f;

    [Header("Recover")]
    public float recoverSpeed = 0.5f;

    [Header("Damage")]
    public float damageInterval = 1f;
    public int damage = 1;

    [Header("Bubble")]
    public GameObject bubblePrefab;
    public float spawnRadius = 6f;
    public float spawnInterval = 1f;
    public int bubblesPerWave = 2;

    [Header("Wall")]
    public LayerMask wallLayer;
    public int maxTry = 20;

    [Header("UI")]
    public GameObject effectBar;
    public Image effectFill;

    [Header("Post Processing Visual")]
    public Volume BlueVolume; // Kéo Global Volume vào đây
    [Range(0f, 1f)]
    public float maxVignetteIntensity = 0.5f; // Độ đậm tối đa của viền xanh
    private Vignette blueVignette;

    private Transform player;
    private PlayerHealth playerHealth;

    private bool playerInside;
    private bool insideBubble;

    private float value;

    private Coroutine bubbleRoutine;
    private Coroutine damageRoutine;

    private void Start()
    {
        if (effectBar != null)
            effectBar.SetActive(false);

        if (effectFill != null)
            effectFill.fillAmount = 0;

        if (BlueVolume != null)
        {
            BlueVolume.weight = 0f; // Mặc định ẩn Volume
            if (BlueVolume.profile != null)
            {
                BlueVolume.profile = Instantiate(BlueVolume.profile);
                if (BlueVolume.profile.TryGet(out blueVignette))
                {
                    blueVignette.intensity.overrideState = true;
                    blueVignette.intensity.value = 0f;
                }
            }
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        // Thanh oxy
        if (insideBubble)
            value -= recoverSpeed * Time.deltaTime;
        else
            value += Time.deltaTime / fillTime;

        value = Mathf.Clamp01(value);

        if (effectFill != null)
            effectFill.fillAmount = value;

        // Hiệu ứng màn hình theo lượng thiếu oxy
        if (blueVignette != null)
        {
            blueVignette.intensity.value = value * maxVignetteIntensity;
        }
    }

    IEnumerator DamageRoutine()
    {
        while (playerInside)
        {
            if (value >= 1f)
            {
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage);

                yield return new WaitForSeconds(damageInterval);
            }
            else
            {
                yield return null;
            }
        }

        damageRoutine = null;
    }

    IEnumerator BubbleRoutine()
    {
        while (playerInside)
        {
            for (int i = 0; i < bubblesPerWave; i++)
            {
                if (TryGetSpawnPoint(out Vector3 pos))
                {
                    Instantiate(bubblePrefab, pos, Quaternion.identity);
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    bool TryGetSpawnPoint(out Vector3 spawnPos)
    {
        for (int i = 0; i < maxTry; i++)
        {
            Vector2 random = Random.insideUnitCircle * spawnRadius;

            Vector3 target = player.position + (Vector3)random;

            Vector2 dir = target - player.position;

            RaycastHit2D hit = Physics2D.Raycast(
                player.position,
                dir.normalized,
                dir.magnitude,
                wallLayer);

            if (hit.collider == null)
            {
                spawnPos = target;
                return true;
            }
        }

        spawnPos = Vector3.zero;
        return false;
    }

    public void SetInsideBubble(bool inside)
    {
        insideBubble = inside;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        player = other.transform;
        playerHealth = other.GetComponent<PlayerHealth>();

        if (BlueVolume != null)
            BlueVolume.weight = 1f;

        // 1. Kích hoạt Coroutine spawn bong bóng
        if (bubbleRoutine == null)
            bubbleRoutine = StartCoroutine(BubbleRoutine());

        // 2. BỔ SUNG: Kích hoạt Coroutine gây sát thương khi ngộp thở
        if (damageRoutine == null)
            damageRoutine = StartCoroutine(DamageRoutine());

        if (effectBar != null)
            effectBar.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        insideBubble = false;

        value = 0;

        if (effectFill != null)
            effectFill.fillAmount = 0;

        if (effectBar != null)
            effectBar.SetActive(false);

        if (bubbleRoutine != null)
            StopCoroutine(bubbleRoutine);

        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        if (blueVignette != null)
            blueVignette.intensity.value = 0f;

        if (BlueVolume != null)
            BlueVolume.weight = 0f; // Tắt Volume vùng nước khi rời đi

        bubbleRoutine = null;
        damageRoutine = null;
    }
}