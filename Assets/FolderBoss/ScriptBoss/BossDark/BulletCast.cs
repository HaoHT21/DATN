using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletCast : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;

    [Header("Damage")]
    public int damage = 20;

    [Header("Life Time")]
    public float lifeTime = 5f;

    [Header("Collider")]
    public BoxCollider2D detectCollider;
    public BoxCollider2D damageCollider;

    private bool warningPhase;

    private bool lockedPosition;

    [Header("Animation")]
    public Animator anim;

    private bool isHit;

    private Transform target;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponent<Animator>();
    }

    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy Player");
        }

        // Ẩn collider gây damage
        if (damageCollider != null)
            damageCollider.enabled = false;

        if (lifeTime > 0)
            Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (lockedPosition)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (target == null)
            return;

        else if (!lockedPosition)
        {
            Vector2 direction =
                (target.position - transform.position)
                .normalized;

            rb.linearVelocity =
                direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!warningPhase)
        {
            warningPhase = true;

            rb.linearVelocity = Vector2.zero;
            lockedPosition = true;

            StartCoroutine(WarningPhase());

            return;
        }

        if (isHit)
            return;

        isHit = true;

        PlayerHealth player =
            other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
        }

        if (damageCollider != null)
            damageCollider.enabled = false;

        if (detectCollider != null)
            detectCollider.enabled = false;

        StartCoroutine(DestroyAfterCast());
    }

    private IEnumerator WarningPhase()
    {
        // Thời gian cảnh báo
        yield return new WaitForSeconds(0.5f);

        anim.Play("bulletcast");

        // Đợi animation chạy trước khi gây damage
        yield return new WaitForSeconds(0.2f);

        if (damageCollider != null)
            damageCollider.enabled = true;

        yield return new WaitForSeconds(0.4f);

        if (damageCollider != null)
            damageCollider.enabled = false;

        if (detectCollider != null)
            detectCollider.enabled = false;
    }   

    private System.Collections.IEnumerator DestroyAfterCast()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }


}