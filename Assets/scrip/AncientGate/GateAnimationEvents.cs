using UnityEngine;

public class GateAnimationEvents : MonoBehaviour
{
    public AncientGateController gate;

    private void Awake()
    {
        if (gate == null)
            gate = GetComponentInParent<AncientGateController>();
    }

    /// <summary>
    /// Gắn hàm này vào Animation Event 'ShowPortal' trong cửa sổ Animation tab
    /// </summary>
    public void ShowPortal()
    {
        if (gate != null)
        {
            gate.ShowActivePortal();
        }
    }

    /// <summary>
    /// Hoặc gắn hàm này ở FRAME CUỐI CÙNG của Animation "Open"
    /// </summary>
    public void OnOpenAnimationComplete()
    {
        if (gate != null)
        {
            gate.ShowActivePortal();
        }
    }
}