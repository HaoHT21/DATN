using UnityEngine;

public class LightningSkillEffect : MonoBehaviour
{
    public float lifetime = 0.5f; // Thời gian tia sét tồn tại (khoảng nửa giây là chạy xong animation)

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}