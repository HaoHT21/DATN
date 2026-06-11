using UnityEngine;

/// <summary>
/// Gắn vào GameObject có Animator của cổng, gọi từ Animation Event khi mở xong.
/// </summary>
public class GateAnimationEvents : MonoBehaviour
{
    public AncientGateController gate;

    private void Awake()
    {
        if (gate == null)
            gate = GetComponentInParent<AncientGateController>();
    }

    public void OnOpenAnimationComplete()
    {
        gate?.OnOpenAnimationComplete();
    }
}
