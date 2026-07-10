using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletCircleSpawner : MonoBehaviour
{
    [Header("Main Bullet")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 20;

    [Header("Circle Bullet")]
    public GameObject circleBulletPrefab;

    public int spawnCount = 8;
    public float spawnInterval = .3f;
    public float circleBulletSpeed = 3f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        moveDirection =
            transform.right;

        StartCoroutine(
            SpawnCircle()
        );

        Destroy(
            gameObject,
            lifeTime
        );
    }

    void FixedUpdate()
    {
        rb.MovePosition(
            rb.position +
            moveDirection *
            speed *
            Time.fixedDeltaTime
        );
    }

    //--------------------------------
    // SPAM TRÒN 360
    //--------------------------------

    IEnumerator SpawnCircle()
    {
        while (true)
        {
            SpawnCircleBullets();

            yield return
            new WaitForSeconds(
                spawnInterval
            );
        }
    }

    void SpawnCircleBullets()
    {
        if (
            circleBulletPrefab == null
        )
            return;

        float angleStep =
            360f / spawnCount;

        for (
            int i = 0;
            i < spawnCount;
            i++
        )
        {
            float angle =
                i * angleStep;

            float rad =
                angle *
                Mathf.Deg2Rad;

            Vector2 dir =
            new Vector2(
                Mathf.Cos(rad),
                Mathf.Sin(rad)
            );

            GameObject bullet =
            Instantiate(
                circleBulletPrefab,
                transform.position,
                Quaternion.Euler(
                    0,
                    0,
                    angle
                )
            );

            Rigidbody2D bulletRb =
            bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null)
            {
                bulletRb.linearVelocity =
                    dir *
                    circleBulletSpeed;
            }

            // truyền damage sang đạn con
            BulletDamage damageScript =
                bullet.GetComponent<
                BulletDamage>();

            if (damageScript != null)
            {
                damageScript.damage =
                    damage;
            }
        }
    }

    //--------------------------------
    // COLLISION
    //--------------------------------

    void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (
            other.CompareTag(
                "Wall"
            )
        )
        {
            Destroy(gameObject);
        }

        if (
            other.CompareTag(
                "Player"
            )
        )
        {
            PlayerHealth player =
            other.GetComponent<
                PlayerHealth>();

            if (player != null)
            {
                Vector2 hitDirection =
           moveDirection.normalized;

                player.TakeDamage(
                    damage, 
                    hitDirection
                );
            }

            Destroy(gameObject);
        }
    }
}