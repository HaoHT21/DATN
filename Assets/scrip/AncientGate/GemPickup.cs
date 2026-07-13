using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GemPickup : MonoBehaviour
{
    [Header("Ngọc")]
    public ItemData gemData;

    [Header("Âm thanh")]
    public AudioClip pickupSound;

    private bool _collected;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected || !other.CompareTag("Player"))
            return;

        if (gemData == null)
        {
            Debug.LogWarning($"[GemPickup] {name} chưa gán ItemData.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[GemPickup] Không tìm thấy InventoryManager trong scene.");
            return;
        }

        if (!InventoryManager.Instance.AddItem(gemData))
            return;

        _collected = true;
        PlaySound(pickupSound);
        if (TryGetComponent<CollectibleSaveable>(out var collectible))
            collectible.Collect();
        else
            Destroy(gameObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }
}
