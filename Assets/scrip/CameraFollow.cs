using UnityEngine;
using Unity.Cinemachine; // Nếu bạn dùng Cinemachine v3+
// using Cinemachine;    // Mở comment dòng này nếu bạn đang dùng Cinemachine v2.x cũ hơn

public class CinemachineTargetAutoTracker : MonoBehaviour
{
    [Header("Cinemachine References")]
    [Tooltip("Kéo Cinemachine Camera / Virtual Camera của bạn vào đây")]
    public CinemachineCamera cinemachineCam;
    // Nếu dùng Cinemachine v2.x, hãy đổi 'CinemachineCamera' thành 'CinemachineVirtualCamera'

    [Header("Auto Search Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float searchInterval = 0.5f;

    private Transform currentTarget;
    private float nextSearchTime = 0f;

    private void Awake()
    {
        // Tự động lấy component CinemachineCamera trên chính GameObject này nếu chưa gán
        if (cinemachineCam == null)
        {
            cinemachineCam = GetComponent<CinemachineCamera>();
            // Nếu dùng v2.x: cinemachineCam = GetComponent<CinemachineVirtualCamera>();
        }
    }

    private void Update()
    {
        // Nếu mục tiêu hiện tại bị mất (đã destroyed hoặc chuyển cảnh)
        if (currentTarget == null)
        {
            if (Time.time >= nextSearchTime)
            {
                FindAndAssignPlayer();
                nextSearchTime = Time.time + searchInterval; // Giảm tần suất tìm kiếm để tối ưu FPS
            }
        }
    }

    /// <summary>
    /// Gọi hàm này khi bạn chủ động đổi nhân vật trong game
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
        UpdateCinemachineTarget(currentTarget);
    }

    private void FindAndAssignPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            currentTarget = playerObj.transform;
            UpdateCinemachineTarget(currentTarget);
        }
    }

    private void UpdateCinemachineTarget(Transform targetTransform)
    {
        if (cinemachineCam == null) return;

        // Cập nhật Tracking Target cho Cinemachine Camera (v3+)
        cinemachineCam.Target.TrackingTarget = targetTransform;

        /* Nếu bạn đang dùng Cinemachine v2.x cũ, đổi dòng trên thành:
        cinemachineCam.Follow = targetTransform;
        cinemachineCam.LookAt = targetTransform; // Nếu muốn camera xoay nhìn theo
        */
    }
}