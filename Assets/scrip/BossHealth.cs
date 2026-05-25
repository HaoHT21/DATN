using Fusion;
using UnityEngine;

public class BossHealth : NetworkBehaviour
{
    [Header("Chỉ số Boss")]
    [Networked] public int CurrentHP { get; set; }
    public int MaxHP = 500;

    public override void Spawned()
    {
        // Chỉ máy Host mới có quyền khởi tạo máu ban đầu
        if (Object.HasStateAuthority)
        {
            CurrentHP = MaxHP;
        }
    }

    // Hàm nhận sát thương đồng bộ mạng (RPC)
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_TakeDamage(int damage, string playerName)
    {
        if (CurrentHP <= 0) return;

        CurrentHP -= damage;

        // Hiện thông báo trong Console để Trung test
        Debug.Log($"<color=red>[BOSS]</color> Bị {playerName} chém! Máu còn lại: {CurrentHP}/{MaxHP}");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Debug.Log("<color=black><b>[BOSS DEFEATED]</b></color> Boss đã gục ngã!");

            // Gọi biến IsDead bên script BossAI để chạy Animation chết
            if (TryGetComponent<BossAI>(out var ai))
            {
                ai.IsDead = true;
            }
        }
    }
}