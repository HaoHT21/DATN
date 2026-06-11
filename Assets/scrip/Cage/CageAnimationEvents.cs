using UnityEngine;

/// <summary>
/// Gắn vào GameObject có Animator của lồng.
/// Tạo Animation Event tại frame cuối animation mở cửa, gọi OnCageOpenComplete().
/// </summary>
public class CageAnimationEvents : MonoBehaviour
{
    [SerializeField] private CageController cageController;

    private void Awake()
    {
        if (cageController == null)
            cageController = GetComponentInParent<CageController>();
    }

    public void OnCageOpenComplete()
    {
        if (cageController != null)
            cageController.OnOpenAnimationComplete();
    }
}
