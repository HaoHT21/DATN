using UnityEngine;

public class FireLookAtPlayer : MonoBehaviour
{
    public Transform target;
    public float rotateSpeed = 15f;
    public LayerMask obstacleMask;

    private bool isTracking = false;
    private Vector3 lockedPosition; // Lưu vị trí tĩnh của Player tại thời điểm bắt đầu ngắm

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

        // Xoay về vị trí ĐÃ KHÓA (lockedPosition) chứ không theo đuổi target.position liên tục
        RotateTowardsPoint(lockedPosition, false);
    }

    // Bắt đầu ngắm: Khóa luôn vị trí hiện tại của Player làm "vị trí cũ"
    public void StartTracking()
    {
        if (target != null)
        {
            lockedPosition = target.position; // Chụp lại vị trí lúc này!
        }
        isTracking = true;
    }

    public void StopTracking()
    {
        if (isTracking)
        {
            RotateTowardsPoint(lockedPosition, true); // Chốt góc chính xác vào vị trí đã khóa
            isTracking = false;
        }
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
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation;

        if (direction.x < 0)
        {
            targetRotation = Quaternion.Euler(180f, 0f, -angle);
        }
        else
        {
            targetRotation = Quaternion.Euler(0f, 0f, angle);
        }

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