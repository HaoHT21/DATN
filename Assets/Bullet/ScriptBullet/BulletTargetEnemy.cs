using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletTargetEnemy : MonoBehaviour
{
    [Header("Bullet")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 10;

    [Header("Delay")]
    public float aimDelay = 1f;

    private Rigidbody2D rb;

    private Vector2 moveDirection;
    private bool canMove;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        StartCoroutine(AimAndMove());

        Destroy(gameObject, lifeTime);
    }

    IEnumerator AimAndMove()
    {
        canMove = false;

        yield return new WaitForSeconds(aimDelay);

        //--------------------------------
        // TÌM MỤC TIÊU GẦN NHẤT
        //--------------------------------

        Transform target = FindNearestTarget();

        if (target == null)
            yield break;

        //--------------------------------
        // KHÓA HƯỚNG
        //--------------------------------

        moveDirection =
            (target.position - transform.position).normalized;

        //--------------------------------
        // XOAY NHÌN MỤC TIÊU
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
    // TÌM ENEMY/BOSS GẦN NHẤT
    //--------------------------------

    Transform FindNearestTarget()
    {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            float dis =
                Vector2.Distance(
                    transform.position,
                    enemy.transform.position);

            if (dis < minDistance)
            {
                minDistance = dis;
                nearest = enemy.transform;
            }
        }

        GameObject[] bosses =
            GameObject.FindGameObjectsWithTag("Boss");

        foreach (GameObject boss in bosses)
        {
            float dis =
                Vector2.Distance(
                    transform.position,
                    boss.transform.position);

            if (dis < minDistance)
            {
                minDistance = dis;
                nearest = boss.transform;
            }
        }

        return nearest;
    }

    //--------------------------------
    // VA CHẠM
    //--------------------------------

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy =
                other.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Boss"))
        {
            BossHeath boss =
                other.GetComponent<BossHeath>();

            if (boss != null)
            {
                boss.TakeDamage(
                    damage
                );
            }

            Destroy(gameObject);
        }
    }
}