using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    public float rotateSpeed = 15f;
    public float detectRadius = 8f;

    private float originalDetectRadius; // Lưu lại giá trị ban đầu (vd: 8)
    private Transform target;
    private PlayerController player;

    [Header("Vision")]
    public LayerMask wallLayer;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
        originalDetectRadius = detectRadius; // Ghi nhớ bán kính phát hiện gốc
    }

    void Update()
    {
        // 1. Luôn tìm kẻ địch gần nhất còn sống và không bị che bởi tường
        FindNearestTarget();

        Vector2 direction;

        // 2. Nếu tìm thấy Enemy -> Nhìn theo Enemy
        if (target != null)
        {
            direction = target.position - transform.position;
        }
        // 3. Không có Enemy -> Nhìn theo hướng di chuyển của người chơi
        else
        {
            direction = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            // Nếu đứng yên và không có kẻ địch thì giữ nguyên góc hiện tại
            if (direction.magnitude < 0.1f)
                return;
        }

        // 4. Tính toán góc quay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 5. Quay mượt mà theo thời gian
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            rotation,
            rotateSpeed * Time.deltaTime
        );

        // 6. Flip lại Sprite nếu hướng quay sang trái
        Vector3 scale = transform.localScale;
        if (direction.x < 0)
            scale.y = -Mathf.Abs(scale.y);
        else
            scale.y = Mathf.Abs(scale.y);

        transform.localScale = scale;
    }

    /// <summary>
    /// Thay đổi bán kính nhắm kẻ địch khi bị tối om
    /// </summary>
    public void SetCustomDetectRadius(float customRadius)
    {
        detectRadius = customRadius;
    }

    /// <summary>
    /// Trả bán kính nhắm kẻ địch về lại giá trị ban đầu khi có ánh sáng
    /// </summary>
    public void ResetDetectRadius()
    {
        detectRadius = originalDetectRadius;
    }

    void FindNearestTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        FindTargetByTag("Enemy", ref closest, ref closestDistance);
        FindTargetByTag("Boss", ref closest, ref closestDistance);

        target = closest;
    }

    void FindTargetByTag(string tag, ref Transform closest, ref float closestDistance)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in objects)
        {
            if (!obj.activeInHierarchy) continue;

            float distance = Vector2.Distance(transform.position, obj.transform.position);

            if (distance <= detectRadius && distance < closestDistance && CanSeeTarget(obj.transform))
            {
                closestDistance = distance;
                closest = obj.transform;
            }
        }
    }

    private bool CanSeeTarget(Transform enemy)
    {
        RaycastHit2D hit = Physics2D.Linecast(
            transform.position,
            enemy.position,
            wallLayer
        );

        return hit.collider == null;
    }

    private void OnDrawGizmos()
    {
        if (target != null)
        {
            Gizmos.color = CanSeeTarget(target) ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}