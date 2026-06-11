public static class GemInventoryHelper
{
    public static bool HasGem(ItemData gem)
    {
        if (gem == null || InventoryManager.Instance == null)
            return false;

        return InventoryManager.Instance.HasEnoughItems(gem.itemID, 1);
    }

    public static bool TryConsumeGem(ItemData gem)
    {
        if (gem == null || InventoryManager.Instance == null)
            return false;

        return InventoryManager.Instance.TryRemoveItems(gem.itemID, 1);
    }

    public static string GetGemLabel(ItemData gem)
    {
        if (gem == null || string.IsNullOrEmpty(gem.itemName))
            return "viên ngọc";

        return gem.itemName;
    }
}
