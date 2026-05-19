using Fusion;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [Header("Cấu hình máu")]
    public int maxHealth = 100;
    [Networked] public int currentHealth { get; set; }
    [Networked] public bool IsDead { get; set; }
    [Networked] private TickTimer despawnTimer { get; set; }

    private Animator _animator;
    private ChangeDetector _changes;

    public override void Spawned()
    {
        _animator = GetComponent<Animator>();
        // Khởi tạo bộ theo dõi thay đổi biến mạng
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasStateAuthority)
        {
            currentHealth = maxHealth;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_TakeDamage(int damage)
    {
        if (IsDead || currentHealth <= 0) return;

        currentHealth -= damage;

        // 1. GHI MÁU RA CONSOLE
        Debug.Log($"<color=red>[ENEMY]</color> {gameObject.name} trúng đòn! Máu còn: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        IsDead = true;
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        // Chờ 2 giây để diễn xong Animation Death rồi mới xóa
        despawnTimer = TickTimer.CreateFromSeconds(Runner, 2f);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && IsDead && despawnTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    public override void Render()
    {
        // 2. TỰ ĐỘNG CHẠY ANIMATION KHI BIẾN MẠNG THAY ĐỔI
        foreach (var change in _changes.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(currentHealth):
                    if (currentHealth > 0 && currentHealth < maxHealth)
                    {
                        // Kích hoạt Animation trúng đòn
                        if (_animator != null) _animator.SetTrigger("Hit");
                    }
                    break;

                case nameof(IsDead):
                    if (IsDead && _animator != null)
                    {
                        // Kích hoạt Animation chết
                        _animator.SetBool("isDead", true);
                    }
                    break;
            }
        }
    }
}