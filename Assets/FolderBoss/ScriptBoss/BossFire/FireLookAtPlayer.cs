using UnityEngine;

public class FireLookAtPlayer : MonoBehaviour
{
    public Transform target;
    public float rotateSpeed = 15f;
    public LayerMask obstacleMask;

    private bool isTracking = false;
    private Vector3 lockedPosition; // Vị trí tĩnh của Player được chốt lúc bắt đầu ngắm
    private float lockedBaseAngle;  // Góc Z chốt để làm mốc quét

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        if (!isTracking) return;

        // Giống Laser: Chỉ xoay về vị trí ĐÃ KHÓA (lockedPosition)
        RotateTowardsPoint(lockedPosition, false);
    }

    // 1. BẮT ĐẦU NGẮM: Khóa ngay vị trí hiện tại của Player
    public void StartTracking()
    {
        if (target != null)
        {
            lockedPosition = target.position; // Chụp lại vị trí Player tại frame này!
        }
        isTracking = true;
    }

    // 2. TẮT NGẮM & CHỐT GÓC QUÉT
    public void StopTracking()
    {
        if (isTracking)
        {
            // Xoay lập tức đến điểm đã khóa để loại bỏ độ trễ của Lerp
            RotateTowardsPoint(lockedPosition, true);
            isTracking = false;
        }

        // Lưu góc Z hiện tại làm mốc cố định cho đợt phun lửa quét
        lockedBaseAngle = transform.eulerAngles.z;
    }

    // 3. QUÉT GÓC: Xoay dựa trên góc gốc đã chốt
    public void SetSweepAngle(float offsetAngle)
    {
        float finalAngle = lockedBaseAngle + offsetAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);
    }

    public bool CanSeePlayer()
    {
        if (target == null) return false;

        Vector3 dirToPlayer = target.position - transform.position;
        float distToPlayer = dirToPlayer.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer.normalized, distToPlayer, obstacleMask);

        return hit.collider == null;
    }

    private void RotateTowardsPoint(Vector3 point, bool instant)
    {
        Vector3 direction = point - transform.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        if (instant)
        {
            transform.rotation = targetRotation;
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }
}