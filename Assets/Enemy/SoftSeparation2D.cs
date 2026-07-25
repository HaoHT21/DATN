using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SoftSeparation2D : MonoBehaviour
{
    [Header("Separation Settings")]
    [Tooltip("Tốc độ/Độ mạnh của lực đẩy mềm")]
    [SerializeField] private float separationForce = 3.5f;

    [Tooltip("Bán kính nhận diện để bắt đầu đẩy (tương ứng với radius của CircleCollider2D)")]
    [SerializeField] private float separationRadius = 0.5f;

    [Tooltip("Chỉ đẩy các đối tượng nằm trong Layer này (ví dụ: Enemy, Player)")]
    [SerializeField] private LayerMask separationLayers;

    private CircleCollider2D triggerCollider;
    private readonly List<Transform> overlappingEntities = new List<Transform>();

    private void Awake()
    {
        // Tự động thiết lập Trigger Collider dùng riêng cho lực đẩy mềm
        triggerCollider = GetComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = separationRadius;
    }

    private void Update()
    {
        ApplySoftSeparation();
    }

    private void ApplySoftSeparation()
    {
        if (overlappingEntities.Count == 0) return;

        Vector2 totalSeparationVector = Vector2.zero;

        // Duyệt qua tất cả các đối tượng đang đứng quá gần
        for (int i = overlappingEntities.Count - 1; i >= 0; i--)
        {
            Transform other = overlappingEntities[i];

            // Nếu đối tượng bị xoá (Destroy) trong lúc chơi thì xóa khỏi danh sách
            if (other == null || !other.gameObject.activeInHierarchy)
            {
                overlappingEntities.RemoveAt(i);
                continue;
            }

            // Tính hướng và khoảng cách giữa 2 đối tượng
            Vector2 directionToMe = (transform.position - other.position);
            float distance = directionToMe.magnitude;

            // Nếu đi quá sâu vào trong nhau (khoảng cách nhỏ hơn tổng bán kính)
            if (distance < separationRadius && distance > 0.001f)
            {
                // Khoảng cách càng gần thì lực đẩy càng mạnh (Tỷ lệ nghịch)
                float pushStrength = (separationRadius - distance) / separationRadius;
                totalSeparationVector += directionToMe.normalized * pushStrength;
            }
        }

        // Dịch chuyển vị trí nhẹ nhàng bằng lực trượt mềm
        if (totalSeparationVector != Vector2.zero)
        {
            transform.position += (Vector3)totalSeparationVector * (separationForce * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem Collider đi vào có thuộc Layer cần đẩy hay không
        if (((1 << other.gameObject.layer) & separationLayers) != 0)
        {
            if (!overlappingEntities.Contains(other.transform))
            {
                overlappingEntities.Add(other.transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (overlappingEntities.Contains(other.transform))
        {
            overlappingEntities.Remove(other.transform);
        }
    }

    // Vẽ hình bán kính đẩy trong Scene view để dễ căn chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}