using System.Collections;
using UnityEngine;

public class BulletLaser : MonoBehaviour
{
    [Header("Time")]
    public float aimTime = 1f;      // Thời gian ngắm
    public float showTime = 1f;     // Hiện object trong bao lâu

    [Header("Delay")]
    public float spawnDelay = .5f;

    [Header("Laser")]
    public LineRenderer line;
    public LayerMask hitLayer;      // Player + Wall

    public int damage = 10;

    [Header("Warning")]
    public GameObject warningHeartPrefab;
    private GameObject warningHeart;

    private Transform player;
    private bool lockRotation;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;

        line.enabled = false;

        warningHeart =
        Instantiate(
            warningHeartPrefab,
            transform.position,
            Quaternion.identity
        );

        warningHeart.transform.SetParent(transform);
        warningHeart.transform.localPosition = Vector3.zero;

        StartCoroutine(LockRoutine());
    }

    void Update()
    {
        if (lockRotation)
            return;

        if (player == null)
            return;

        Vector2 direction = player.position - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    IEnumerator LockRoutine()
    {
        //--------------------------------
        // Ngắm player
        //--------------------------------

        yield return new WaitForSeconds(
            aimTime
        );

        //--------------------------------
        // Khóa hướng hiện tại
        //--------------------------------

        lockRotation = true;

        //--------------------------------
        // Delay trước khi bắn
        //--------------------------------

        yield return new WaitForSeconds(
            spawnDelay
        );

        //--------------------------------
        // Bắn
        //--------------------------------

        if (warningHeart != null)
            Destroy(warningHeart);

        FireLaser();

        //--------------------------------
        // Giữ laser
        //--------------------------------

        yield return new WaitForSeconds(
            showTime
        );

        //--------------------------------
        // Hủy
        //--------------------------------

        Destroy(
            gameObject
        );
    }

    void FireLaser()
    {
        line.enabled = true;

        Vector2 dir = transform.right;

        RaycastHit2D hit =
            Physics2D.Raycast(
                transform.position,
                dir,
                50f,
                hitLayer
            );

        Vector3 endPoint;

        if (hit)
        {
            endPoint = hit.point;

            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth hp =
                    hit.collider.GetComponent<PlayerHealth>();

                if (hp != null)
                    hp.TakeDamage(damage);
            }
        }
        else
        {
            endPoint =
                transform.position +
                (Vector3)dir * 50f;
        }

        line.positionCount = 2;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, endPoint);
    }
}
