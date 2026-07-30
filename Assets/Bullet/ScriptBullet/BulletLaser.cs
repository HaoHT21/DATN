using System.Collections;
using UnityEngine;

public class BulletLaser : MonoBehaviour
{
    [Header("Time")]
    public float aimTime = 1f;       // Thời gian ngắm
    public float showTime = 1f;      // Hiện laser trong bao lâu

    [Header("Delay")]
    public float spawnDelay = .5f;

    [Header("Laser & Damage")]
    public LineRenderer line;
    public LayerMask hitLayer;       // Player + Wall
    public int damage = 10;

    [Tooltip("Khoảng thời gian giữa mỗi lần gây sát thương (tính bằng giây)")]
    public float damageInterval = 0.2f; // Ví dụ: 0.2s nhận dame 1 lần
    private float lastDamageTime;

    [Header("Warning")]
    public GameObject warningHeartPrefab;
    private GameObject warningHeart;

    private Transform player;
    private bool lockRotation;
    private bool isFiring = false;   // Đánh dấu laser đang bật

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;

        line.enabled = false;

        if (warningHeartPrefab != null)
        {
            warningHeart = Instantiate(
                warningHeartPrefab,
                transform.position,
                Quaternion.identity
            );
            warningHeart.transform.SetParent(transform);
            warningHeart.transform.localPosition = Vector3.zero;
        }

        StartCoroutine(LockRoutine());
    }

    void Update()
    {
        // Phase 1: Xoay ngắm Player trước khi khóa góc
        if (!lockRotation && player != null)
        {
            Vector2 direction = player.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // Phase 2: Khi Laser đang bật, duy trì vẽ và kiểm tra gây dame liên tục
        if (isFiring)
        {
            UpdateAndDamageLaser();
        }
    }

    IEnumerator LockRoutine()
    {
        // Ngắm player
        yield return new WaitForSeconds(aimTime);

        // Khóa hướng hiện tại
        lockRotation = true;

        // Delay trước khi bắn
        yield return new WaitForSeconds(spawnDelay);

        // Chuẩn bị bắn
        if (warningHeart != null)
            Destroy(warningHeart);

        // Bật laser
        line.enabled = true;
        isFiring = true;

        // Giữ laser trong thời gian showTime
        yield return new WaitForSeconds(showTime);

        // Tắt laser và xóa object
        isFiring = false;
        line.enabled = false;

        Destroy(gameObject);
    }

    void UpdateAndDamageLaser()
    {
        Vector2 dir = transform.right;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            dir,
            100f,
            hitLayer
        );

        Vector3 endPoint;

        if (hit)
        {
            endPoint = hit.point;

            // Kiểm tra va chạm với Player
            if (hit.collider.CompareTag("Player"))
            {
                // Kiểm tra xem đã đến thời gian gây dame tiếp theo chưa
                if (Time.time >= lastDamageTime + damageInterval)
                {
                    PlayerHealth hp = hit.collider.GetComponent<PlayerHealth>();
                    if (hp != null)
                    {
                        hp.TakeDamage(damage);
                        lastDamageTime = Time.time; // Cập nhật lại mốc thời gian gây dame
                    }
                }
            }
        }
        else
        {
            endPoint = transform.position + (Vector3)dir * 100f;
        }

        // Cập nhật lại vị trí vẽ LineRenderer liên tục
        line.positionCount = 2;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, endPoint);
    }
}