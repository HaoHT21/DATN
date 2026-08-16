using UnityEngine;

public class LaserLookAtPlayer : MonoBehaviour
{
    [Header("Tracking & Obstacle Check")]
    public Transform target;
    public LayerMask obstacleMask;
    public float rotateSpeed = 15f;

    [Header("Laser Rotation Settings")]
    public float laserRotateSpeed = 180f; // Tốc độ xoay khi xả Laser (độ/giây)
    public bool rotateClockwise = true;   // Chiều xoay

    private bool isTrackingPlayer = true; // Bình thường liên tục xoay theo Player
    private bool isFiringLaser = false;

    private float currentAngle = 0f;

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
        // 1. Trạng thái bình thường: Xoay nhìn theo Player để cho ShootSkill bắn chuẩn
        if (isTrackingPlayer && target != null)
        {
            RotateTowardsPoint(target.position, false);
        }
        // 2. Trạng thái xả Laser: Xoay tròn liên tục
        else if (isFiringLaser)
        {
            float dir = rotateClockwise ? -1f : 1f;
            currentAngle += dir * laserRotateSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }

    //------------------------------------------------------------------------
    // Called by BossRockController: Bắt đầu Telegraph ngắm 4 hướng ngẫu nhiên
    //------------------------------------------------------------------------
    public void StartTelegraph()
    {
        isTrackingPlayer = false;
        isFiringLaser = false;

        if (target != null)
        {
            Vector3 dir = target.position - transform.position;
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Ngẫu nhiên chọn 1 trong 4 nòng (0°, 90°, 180°, 270°) chĩa về phía Player
            int randomBarrelIndex = Random.Range(0, 4);
            currentAngle = baseAngle + (randomBarrelIndex * 90f);

            // Gán trực tiếp rotation ngắm ngẫu nhiên
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }

    //------------------------------------------------------------------------
    // Called by BossRockController: Bắt đầu xoay Laser xả đạn
    //------------------------------------------------------------------------
    public void StartLaserRotation(bool randomizeDirection = true)
    {
        isTrackingPlayer = false;

        if (randomizeDirection)
        {
            rotateClockwise = Random.value > 0.5f;
        }

        isFiringLaser = true;
    }

    public void StopLaserRotation()
    {
        isFiringLaser = false;
        isTrackingPlayer = true; // Trở về trạng thái nhìn theo Player bình thường
    }

    //------------------------------------------------------------------------
    // Helper Methods
    //------------------------------------------------------------------------
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
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        if (instant)
        {
            transform.rotation = targetRotation;
            currentAngle = angle;
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            currentAngle = transform.eulerAngles.z; // Cập nhật currentAngle đồng bộ theo transform
        }
    }
}