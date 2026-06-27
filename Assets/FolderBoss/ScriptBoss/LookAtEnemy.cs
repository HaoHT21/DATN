using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    public float rotateSpeed = 5f;
    public float detectRadius = 8f;

    private Transform target;

    void Update()
    {
        // kiểm tra target hiện tại còn hợp lệ không
        if (target != null)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    target.position
                );

            // ra khỏi vùng hoặc chết
            if (distance > detectRadius ||
                !target.gameObject.activeInHierarchy)
            {
                target = null;
            }
        }

        // chưa có target thì tìm
        if (target == null)
        {
            FindNearestTarget();
        }

        // quay
        if (target == null)
            return;

        Vector2 direction =
            target.position -
            transform.position;

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
        float closestDistance =
            Mathf.Infinity;

        Transform closest =
            null;

        // tìm Enemy
        FindTargetByTag(
            "Enemy",
            ref closest,
            ref closestDistance
        );

        // tìm Boss
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
            GameObject.FindGameObjectsWithTag(
                tag
            );

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
                closestDistance =
                    distance;

                closest =
                    obj.transform;
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