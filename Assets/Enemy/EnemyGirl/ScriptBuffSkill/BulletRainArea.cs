using System.Collections;
using UnityEngine;

public class BulletRainArea : MonoBehaviour
{
    public GameObject bulletPrefab;

    public float radius = 4f;

    public float fireInterval = 0.1f;

    private GameObject owner;

    public void Initialize(GameObject player, float lifeTime)
    {
        owner = player;

        Destroy(gameObject, lifeTime);

        StartCoroutine(FireRoutine());
    }

    void Update()
    {
        if (owner != null)
            transform.position = owner.transform.position;
    }

    IEnumerator FireRoutine()
    {
        while (true)
        {
            SpawnRandomBullet();

            yield return new WaitForSeconds(fireInterval);
        }
    }

    void SpawnRandomBullet()
    {
        Vector2 pos =
            (Vector2)transform.position +
            Random.insideUnitCircle * radius;

        Instantiate(
            bulletPrefab,
            pos,
            Quaternion.identity);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);

    }
}