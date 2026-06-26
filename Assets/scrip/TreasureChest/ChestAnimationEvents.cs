using UnityEngine;

/// <summary>
/// Gắn vào GameObject có Animator của rương.
/// Tạo Animation Event tại frame cuối animation Open, gọi OnChestOpenComplete().
/// </summary>
public class ChestAnimationEvents : MonoBehaviour
{
    [SerializeField] private TreasureChest chestController;

    private void Awake()
    {
        if (chestController == null)
            chestController = GetComponentInParent<TreasureChest>();
    }

    public void OnChestOpenComplete()
    {
        if (chestController != null)
            chestController.OnOpenAnimationComplete();
    }
}
