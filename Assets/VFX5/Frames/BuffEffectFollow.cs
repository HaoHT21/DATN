using UnityEngine;

public class BuffEffectFollow : MonoBehaviour
{
    private Transform playerTransform;
    private float duration;
    private float timer;

    public void Setup(Transform player, float buffDuration)
    {
        playerTransform = player;
        duration = buffDuration;
        timer = 0f;
    }

    void Update()
    {
        if (playerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        // Bắt vòng phép liên tục đi theo vị trí của Player (hơi lùi xuống chân một tí)
        transform.position = playerTransform.position + new Vector3(0, -0.3f, 0);

        // Đếm ngược thời gian để tự hủy
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}