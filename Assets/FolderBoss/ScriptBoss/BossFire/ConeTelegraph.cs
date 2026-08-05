using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConeTelegraph : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// Vẽ hình quạt tam giác màu đỏ báo động
    /// </summary>
    /// <param name="fov">Góc quét (ví dụ: 90 độ)</param>
    /// <param name="viewDistance">Chiều dài tia lửa</param>
    public void DrawCone(float fov, float viewDistance)
    {
        int rayCount = 30; // Độ mịn của vòng cung
        float currentAngle = -fov / 2f;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // Góc đỉnh tại vị trí Boss/Súng

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 vertex = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * viewDistance;
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
            currentAngle += angleIncrease;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}