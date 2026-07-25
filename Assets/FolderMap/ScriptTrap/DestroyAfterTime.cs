using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [Header("Cấu hình thời gian")]
    public float lifeTime = 5f; // Thời gian tồn tại (mặc định là 5 giây)

    void Start()
    {
        // Tự động hủy chính Game Object này sau số giây quy định
        Destroy(gameObject, lifeTime);
    }
}