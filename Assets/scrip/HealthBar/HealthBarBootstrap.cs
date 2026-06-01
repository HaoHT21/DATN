using UnityEngine;

/// <summary>
/// Tự tìm PlayerHealth trong scene và bind HUD (tùy chọn thay cho kéo tham chiếu tay).
/// </summary>
public class HealthBarBootstrap : MonoBehaviour
{
    [SerializeField] private HealthBarBinder playerHudBinder;
    [SerializeField] private string playerTag = "Player";

    private void Start()
    {
        if (playerHudBinder == null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
            return;

        var provider = player.GetComponent<PlayerHealth>();
        if (provider != null)
            playerHudBinder.Bind(provider);
    }
}
