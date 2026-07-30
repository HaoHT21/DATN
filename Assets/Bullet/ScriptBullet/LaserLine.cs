using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserLine : MonoBehaviour
{
    [Header("Laser Settings")]
    public float maxDistance = 100f;     // Khoảng cách chiếu tối đa
    public LayerMask wallLayer;         // Layer của tường/vật cản

    [Header("Visual Settings")]
    public Color laserColor = Color.red;
    public float laserWidth = 0.1f;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;

        // Tạo Material cơ bản nếu chưa có
        if (lineRenderer.material == null || lineRenderer.material.name.Contains("Default"))
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        lineRenderer.startColor = laserColor;
        lineRenderer.endColor = laserColor;
        lineRenderer.useWorldSpace = true; // Dùng tọa độ thế giới để vẽ chính xác
    }

    void Update()
    {
        // 1. Điểm bắt đầu luôn là vị trí hiện tại của Object phát laser
        Vector2 startPoint = transform.position;
        Vector2 direction = transform.right; // Hướng chiếu của Laser (hướng sang phải của Object)

        // 2. Bắn 1 tia Raycast từ nguồn ra xa để tìm điểm chặn
        RaycastHit2D hit = Physics2D.Raycast(startPoint, direction, maxDistance, wallLayer);

        Vector2 endPoint;

        if (hit.collider != null)
        {
            // Nếu đụng Wall -> Điểm cuối laser dừng ngay tại bề mặt Wall
            endPoint = hit.point;
        }
        else
        {
            // Nếu không vướng Wall -> Điểm cuối kéo dài hết tầm maxDistance
            endPoint = startPoint + direction * maxDistance;
        }

        // 3. Cập nhật vị trí hiển thị cho LineRenderer
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }
}