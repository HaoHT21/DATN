using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletTagret : MonoBehaviour
{
    [Header("Bullet")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 10;

    [Header("Delay")]
    public float aimDelay = 1f; // thời gian đứng yên trước khi bay

    private Rigidbody2D rb;

    private Vector2 moveDirection;
    private bool canMove;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        StartCoroutine(
            AimAndMove()
        );

        Destroy(
            gameObject,
            lifeTime
        );
    }

    IEnumerator AimAndMove()
    {
        // đứng yên trước
        canMove = false;

        yield return
        new WaitForSeconds(
            aimDelay
        );

        //--------------------------------
        // TÌM PLAYER
        //--------------------------------

        GameObject player =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (player == null)
            yield break;

        //--------------------------------
        // KHÓA HƯỚNG
        //--------------------------------

        moveDirection =
        (
            player.transform.position -
            transform.position
        ).normalized;

        //--------------------------------
        // XOAY NHÌN PLAYER
        //--------------------------------

        float angle =
            Mathf.Atan2(
                moveDirection.y,
                moveDirection.x
            ) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(
                0,
                0,
                angle
            );
        //--------------------------------
        // CHO PHÉP BAY
        //--------------------------------

        canMove = true;
    }

    void FixedUpdate()
    {
        if (!canMove)
            return;

        rb.MovePosition(
            rb.position +
            moveDirection *
            speed *
            Time.fixedDeltaTime
        );
    }

    //--------------------------------
    // VA CHẠM
    //--------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (
            other.CompareTag("Wall")
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

        if (other.TryGetComponent<IDamageable>(out var damageableTarget))
        {
            damageableTarget.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }
}