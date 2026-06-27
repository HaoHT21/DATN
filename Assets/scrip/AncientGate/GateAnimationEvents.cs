using UnityEngine;

/// <summary>
/// Giữ lại để tương thích Animation Event cũ trên prefab cổng (không còn dùng).
/// </summary>
public class GateAnimationEvents : MonoBehaviour
{
    public void OnOpenAnimationComplete()
    {
    }
    public AncientGateController gate;

    public void ShowPortal()
    {
        gate.ShowPortal();
    }
}
