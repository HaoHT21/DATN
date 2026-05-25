using Fusion;

using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [Networked] public int currentHealth { get; set; }
    public int maxHealth = 100;
    public bool IsDead;

    public override void Spawned()
    {
        if (Object.HasStateAuthority) currentHealth = maxHealth;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_TakeDamage(int damage)
    {
        if (IsDead || currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"<color=blue>[PLAYER]</color> Trúng đòn! Máu còn: {currentHealth}");

        if (currentHealth <= 0)
        {
            PlayerDie();
        }
    }

    void PlayerDie()
    {
        IsDead = true;
        Debug.Log("<color=black>PLAYER ĐÃ HY SINH!</color>");

        // 3. PLAYER BIẾN MẤT (DESPAWN)
        // Chỉ máy có quyền (StateAuthority) mới được thực hiện lệnh xóa
        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}