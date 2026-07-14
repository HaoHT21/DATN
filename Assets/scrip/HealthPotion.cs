using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Settings")]
    public int healAmount = 50;
    public int itemID;

    [Header("Visuals")]
    public Sprite itemIcon;
    public GameObject potionVisualPrefab;
    public GameObject pickupPrefab;

    private void Awake()
    {
        SaveableHelpers.EnsureCollectible(gameObject, BuildStableId());
    }

    private string BuildStableId()
    {
        string scene = gameObject.scene.IsValid() ? gameObject.scene.name : "unknown";
        int x = Mathf.RoundToInt(transform.position.x * 100f);
        int y = Mathf.RoundToInt(transform.position.y * 100f);
        return $"health-potion-{scene}-{itemID}-{x}-{y}";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null)
            return;

        GameObject visual = null;
        if (potionVisualPrefab != null && pc.weaponHolder != null)
        {
            visual = Instantiate(potionVisualPrefab, pc.weaponHolder);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(2f, 2f, 2f);
            visual.SetActive(false);
        }

        PlayerController.WeaponItem newPotion = new PlayerController.WeaponItem
        {
            itemID = this.itemID,
            icon = this.itemIcon,
            visualPrefab = visual,
            pickupPrefab = this.pickupPrefab,
            isGun = false,
            isPotion = true,
            healAmount = this.healAmount
        };

        if (pc.inventory != null)
        {
            pc.inventory.Add(newPotion);
            pc.UpdateWeaponVisuals();
        }

        if (TryGetComponent<CollectibleSaveable>(out var collectible))
            collectible.Collect();
        else
            Destroy(gameObject);
    }
}
