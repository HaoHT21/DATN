using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    public float rotateSpeed = 5f;
    public float detectRadius = 8f;

    private Transform target;
    private PlayerController player;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        // Kiểm tra target còn hợp lệ không
        if (target != null)
        {
            float distance = Vector2.Distance(
                transform.position,
                target.position
            );

            if (distance > detectRadius ||
                !target.gameObject.activeInHierarchy)
            {
                target = null;
            }
        }

        // Chưa có target thì tìm
        if (target == null)
        {
            FindNearestTarget();
        }

        Vector2 direction;

        // Có enemy → nhìn enemy
        if (target != null)
        {
            direction =
                target.position -
                transform.position;
        }
        // Không có enemy → nhìn theo hướng di chuyển
        else
        {
            direction =
                new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                );

            // Nếu đứng yên thì giữ góc cũ
            if (direction.magnitude < 0.1f)
                return;
        }

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        Quaternion rotation =
            Quaternion.Euler(
                0,
                0,
                angle
            );

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                rotation,
                rotateSpeed *
                Time.deltaTime
            );
    }

    void FindNearestTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        FindTargetByTag(
            "Enemy",
            ref closest,
            ref closestDistance
        );

        FindTargetByTag(
            "Boss",
            ref closest,
            ref closestDistance
        );

        target = closest;
    }

    void FindTargetByTag(
        string tag,
        ref Transform closest,
        ref float closestDistance)
    {
        GameObject[] objects =
            GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in objects)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    obj.transform.position
                );

            if (
                distance <= detectRadius &&
                distance < closestDistance
            )
            {
                closestDistance = distance;
                closest = obj.transform;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            detectRadius
        );
    }
}