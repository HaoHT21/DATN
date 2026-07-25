using UnityEngine;

public class PoisonArea : MonoBehaviour
{
    [Header("Sát thương trực tiếp")]
    [Tooltip("Vừa giẫm vào vùng độc là mất ngần này máu ngay")]
    public int directDamage = 5;

    [Header("Hiệu ứng độc theo thời gian")]
    public int poisonDamage = 1;
    public float damageInterval = 1f;

    [Header("After Exit")]
    public float poisonDurationAfterExit = 3f;

    [Header("Lifetime")]
    public float lifeTime = 5f;

    private void Start()
    {
        // Vùng độc tự biến mất sau lifeTime (ví dụ: 5 giây)
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<PlayerHealth>(out PlayerHealth player))
        {
            // 1. Trừ máu trực tiếp của bẫy ngay khi vừa chạm
            player.TakeDamage(directDamage);

            // 2. Gắn hiệu ứng độc
            ApplyOrRefreshPoison(player);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Nếu người chơi đứng lỳ trong vũng độc, liên tục làm mới (refresh) thời gian dính độc
        if (other.CompareTag("Player") && other.TryGetComponent<PlayerHealth>(out PlayerHealth player))
        {
            ApplyOrRefreshPoison(player);
        }
    }

    private void ApplyOrRefreshPoison(PlayerHealth player)
    {
        PoisonEffect poison = player.GetComponent<PoisonEffect>();

        if (poison == null)
        {
            poison = player.gameObject.AddComponent<PoisonEffect>();
        }

        // Gọi hàm truyền thông số độc sang cho Player xử lý
        poison.ApplyPoison(poisonDamage, damageInterval, poisonDurationAfterExit);
    }
}