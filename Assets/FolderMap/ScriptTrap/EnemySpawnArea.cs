using System.Collections;
using UnityEngine;

public class EnemySpawnArea : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Spawn Area")]
    public float radius = 5f;

    [Header("Spawn")]
    public int maxEnemy = 10;
    public float spawnInterval = 1f;

    private int currentEnemy;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (currentEnemy < maxEnemy)
        {
            SpawnEnemy();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        Vector2 pos =
            (Vector2)transform.position +
            Random.insideUnitCircle * radius;

        GameObject enemy =
            Instantiate(enemyPrefab, pos, Quaternion.identity);

        currentEnemy++;
    }

    public void EnemyDead()
    {
        currentEnemy--;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}