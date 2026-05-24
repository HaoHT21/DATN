using Fusion;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [Header("Máu")]
    public int maxHealth = 100;

    [Networked] public int currentHealth { get; set; }
    [Networked] public bool IsDead { get; set; }
    [Networked] private TickTimer despawnTimer { get; set; }

    [Header("Drop Coin")]
    public NetworkPrefabRef coinPrefab;
    public int coinAmount = 5;

    private Animator _animator;
    private ChangeDetector _changes;

    public override void Spawned()
    {
        _animator = GetComponent<Animator>();
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasStateAuthority)
        {
            currentHealth = maxHealth;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHealth -= damage;

        Debug.Log("Enemy HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        IsDead = true;

        // Tắt collider
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }

        // Animation chết
        if (_animator != null)
        {
            _animator.SetBool("isDead", true);
        }

        // Spawn coin
        SpawnCoins();

        // Delay trước khi xoá enemy
        despawnTimer = TickTimer.CreateFromSeconds(Runner, 2f);
    }

    void SpawnCoins()
    {
        // Chỉ host được spawn
        if (!Object.HasStateAuthority)
            return;

        for (int i = 0; i < coinAmount; i++)
        {
            // Random vị trí xung quanh enemy
            Vector2 randomOffset = Random.insideUnitCircle * 1.5f;

            Vector3 spawnPos = transform.position + (Vector3)randomOffset;

            Runner.Spawn(
                coinPrefab,
                spawnPos,
                Quaternion.identity
            );
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority &&
            IsDead &&
            despawnTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(currentHealth):

                    if (currentHealth > 0 && currentHealth < maxHealth)
                    {
                        if (_animator != null)
                        {
                            _animator.SetTrigger("Hit");
                        }
                    }

                    break;

                case nameof(IsDead):

                    if (IsDead && _animator != null)
                    {
                        _animator.SetBool("isDead", true);
                    }

                    break;
            }
        }
    }
}