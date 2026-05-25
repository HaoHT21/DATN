using Fusion;
using UnityEngine;

public class BossFireball : NetworkBehaviour
{
    public float speed = 8f;
    public int damage = 20;

    public override void FixedUpdateNetwork()
    {
        // Đạn bay thẳng về phía trước theo trục X (hoặc hướng bạn muốn)
        transform.Translate(Vector3.right * speed * Runner.DeltaTime);

        // Tự hủy sau 3 giây để tránh rác server
        if (Object.HasStateAuthority && TickTimer.CreateFromSeconds(Runner, 3f).Expired(Runner))
            Runner.Despawn(Object);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Object.HasStateAuthority && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerHealth>(out var hp)) hp.Rpc_TakeDamage(damage);
            Runner.Despawn(Object); // Chạm người chơi là biến mất
        }
    }
}