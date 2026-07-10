using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RockZone : MonoBehaviour
{
    [Header("Time")]
    public float fillTime = 8f;
    public float drainTime = 3f;

    [Header("UI")]
    public GameObject effectBar;
    public Image effectFill;

    [Header("Rock")]
    public GameObject warningPrefab;
    public float spawnRadius = 6f;
    public float spawnInterval = 0.6f;

    [Header("Rock Count")]
    public int rocksPerWave = 3;

    private Transform player;

    private bool playerInside;
    private bool isFilling = true;
    private bool rockActive;

    [Header("Wall")]
    public LayerMask wallLayer;

    [Header("Random")]
    public int maxTry = 20;

    private float value;

    private Coroutine spawnRoutine;

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

        if (isFilling)
        {
            value += Time.deltaTime / fillTime;

            if (value >= 1)
            {
                value = 1;

                if (!rockActive)
                {
                    rockActive = true;

                    spawnRoutine =
                        StartCoroutine(SpawnRockRoutine());

                    // CameraShake.Instance.StartShake();
                }

                isFilling = false;
            }
        }
        else
        {
            value -= Time.deltaTime / drainTime;

            if (value <= 0)
            {
                value = 0;

                StopRock();

                isFilling = true;
            }
        }

        if (effectFill != null)
            effectFill.fillAmount = value;
    }

    IEnumerator SpawnRockRoutine()
    {
        while (true)
        {
            for (int i = 0; i < rocksPerWave; i++)
            {
                Vector3 pos;

                if (TryGetSpawnPoint(out pos))
                {
                    Instantiate(
                        warningPrefab,
                        pos,
                        Quaternion.identity);
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private bool TryGetSpawnPoint(out Vector3 spawnPos)
    {
        for (int i = 0; i < maxTry; i++)
        {
            Vector2 random =
                Random.insideUnitCircle * spawnRadius;

            Vector3 target =
                player.position + (Vector3)random;

            Vector2 dir =
                target - player.position;

            float distance = dir.magnitude;

            RaycastHit2D hit =
                Physics2D.Raycast(
                    player.position,
                    dir.normalized,
                    distance,
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

    void StopRock()
    {
        rockActive = false;

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = null;

        // CameraShake.Instance.StopShake();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        player = other.transform;

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

        StopRock();

        if (effectFill != null)
            effectFill.fillAmount = 0;

        if (effectBar != null)
            effectBar.SetActive(false);
    }
}