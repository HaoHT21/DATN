using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapRock : MonoBehaviour
{
    [Header("Trap")]
    public Animator animator;

    [Header("Spawn Area")]
    public GameObject warningPrefab;
    public float spawnRadius = 6f;
    public int rockCount = 8;
    public float spawnDelay = 0.15f;

    [Header("Wall")]
    public LayerMask wallLayer;
    public int maxTry = 20;

    [Header("Distance")]
    public float minDistance = 1.5f;

    private bool activated;

    private readonly List<Vector3> usedPoints = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated)
            return;

        if (!other.CompareTag("Player"))
            return;

        activated = true;

        if (animator != null)
            animator.Play("TrapRock");

        StartCoroutine(SpawnRoutine(other.transform));
    }

    IEnumerator SpawnRoutine(Transform player)
    {
        yield return new WaitForSeconds(0.5f);

        usedPoints.Clear();

        for (int i = 0; i < rockCount; i++)
        {
            if (TryGetSpawnPoint(player, out Vector3 pos))
            {
                usedPoints.Add(pos);

                Instantiate(
                    warningPrefab,
                    pos,
                    Quaternion.identity);

                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }

    bool TryGetSpawnPoint(Transform player, out Vector3 spawnPos)
    {
        for (int i = 0; i < maxTry; i++)
        {
            Vector2 random =
                Random.insideUnitCircle * spawnRadius;

            Vector3 target =
                transform.position + (Vector3)random;

            // Không spawn sau tường
            Vector2 dir =
                target - player.position;

            RaycastHit2D hit =
                Physics2D.Raycast(
                    player.position,
                    dir.normalized,
                    dir.magnitude,
                    wallLayer);

            if (hit.collider != null)
                continue;

            // Không spawn quá gần nhau
            bool overlap = false;

            foreach (Vector3 point in usedPoints)
            {
                if (Vector3.Distance(point, target) < minDistance)
                {
                    overlap = true;
                    break;
                }
            }

            if (overlap)
                continue;

            spawnPos = target;
            return true;
        }

        spawnPos = Vector3.zero;
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Vùng spawn
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Các điểm đã spawn (khi đang Play)
        Gizmos.color = Color.red;

        foreach (Vector3 point in usedPoints)
        {
            Gizmos.DrawSphere(point, 0.2f);
        }
    }
#endif
}