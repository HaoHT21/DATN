using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletTrailSpawner : MonoBehaviour
{
    [Header("Main Bullet")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 10;

    [Header("Trail Bullet")]
    public GameObject trailBulletPrefab;

    public float spawnInterval = .1f;
    public float trailBulletSpeed = 1f;

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
            SpawnTrail()
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
    // SPAM ĐẠN CON
    //--------------------------------

    IEnumerator SpawnTrail()
    {
        while (true)
        {
            SpawnChildBullet();

            yield return
            new WaitForSeconds(
                spawnInterval
            );
        }
    }

    void SpawnChildBullet()
    {
        if (
            trailBulletPrefab ==
            null
        )
            return;

        GameObject bullet =
        Instantiate(
            trailBulletPrefab,
            transform.position,
            transform.rotation
        );

        Rigidbody2D bulletRb =
        bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity =
            moveDirection *
            trailBulletSpeed;
        }
    }

    //--------------------------------
    // COLLISION
    //--------------------------------

    void OnTriggerEnter2D(
        Collider2D other
    )
    {
        //--------------------------------
        // WALL
        //--------------------------------

        if (
            other.CompareTag(
                "Wall"
            )
        )
        {
            Destroy(gameObject);
        }

        //--------------------------------
        // PLAYER
        //--------------------------------

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

        if (other.TryGetComponent<IDamageable>(out var damageableTarget))
        {
            damageableTarget.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }
}