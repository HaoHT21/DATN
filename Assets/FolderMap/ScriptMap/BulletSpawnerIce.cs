using UnityEngine;

public class BulletSpawnerIce : MonoBehaviour
{
    [Header("Health")]
    public int hp = 1;

    [Header("Bullet")]
    public GameObject bulletPrefab;

    [Header("Spawn")]
    public int bulletCount = 8;
    public float bulletSpeed = 5f;

    private bool activated;

    public void TakeDamage(int damageAmount)
    {
        if (activated)
            return;

        hp -= damageAmount;

        if (hp <= 0)
        {
            SpawnBullets();
        }
    }

    private void SpawnBullets()
    {
        activated = true;

        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;

            Quaternion rotation =
                Quaternion.Euler(0, 0, angle);

            GameObject bullet =
                Instantiate(
                    bulletPrefab,
                    transform.position,
                    rotation);

            Rigidbody2D rb =
                bullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity =
                    bullet.transform.right *
                    bulletSpeed;
            }
        }

        Destroy(gameObject);
    }

    public void OnBulletHit(int damage)
    {
        TakeDamage(damage);
    }
}