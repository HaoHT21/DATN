using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    public float rotateSpeed = 15f; // Tăng tốc độ xoay lên một chút để xoay mượt và nhanh hơn
    public float detectRadius = 8f;

    private Transform target;
    private PlayerController player;

    [Header("Vision")]
    public LayerMask wallLayer;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
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

        // 6. Flip lại Sprite nếu hướng quay sang trái để tránh bị ngửa bụng (cho súng/mắt 2D)
        Vector3 scale = transform.localScale;
        if (direction.x < 0)
            scale.y = -Mathf.Abs(scale.y);
        else
            scale.y = Mathf.Abs(scale.y);

        transform.localScale = scale;
    }

    void FindNearestTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        // Tìm trong nhóm Enemy
        FindTargetByTag("Enemy", ref closest, ref closestDistance);

        // Tìm trong nhóm Boss
        FindTargetByTag("Boss", ref closest, ref closestDistance);

        // Cập nhật target mới nhất (luôn là kẻ gần nhất)
        target = closest;
    }

    void FindTargetByTag(string tag, ref Transform closest, ref float closestDistance)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in objects)
        {
            // Bỏ qua nếu GameObject bị ẩn/đã chết
            if (!obj.activeInHierarchy) continue;

            float distance = Vector2.Distance(transform.position, obj.transform.position);

            // Điều kiện để chọn: 
            // - Nằm trong tầm detectRadius
            // - Gần hơn khoảng cách của kẻ địch hiện tại
            // - Không bị tường che
            if (distance <= detectRadius && distance < closestDistance && CanSeeTarget(obj.transform))
            {
                closestDistance = distance;
                closest = obj.transform;
            }
        }
    }

    private bool CanSeeTarget(Transform enemy)
    {
        // Kiểm tra xem có bức tường nào chắn giữa Player và Enemy không
        RaycastHit2D hit = Physics2D.Linecast(
            transform.position,
            enemy.position,
            wallLayer
        );

        // Nếu collider trả về null tức là không đụng tường -> Nhìn thấy được
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