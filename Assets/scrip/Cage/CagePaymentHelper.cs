public static class CagePaymentHelper
{
    public static int GetCurrentAmount(CageRequirement requirement)
    {
        if (requirement == null)
            return 0;

        switch (requirement.type)
        {
            case CageRequirementType.Coin:
                return PlayerStats.Instance != null ? PlayerStats.Instance.CoinCount : 0;

            case CageRequirementType.Item:
                return InventoryManager.Instance != null
                    ? InventoryManager.Instance.CountItem(requirement.ItemID)
                    : 0;

            default:
                return 0;
        }
    }

    public static bool CanAfford(CageRequirement requirement)
    {
        if (requirement == null || requirement.amount <= 0)
            return false;

        return GetCurrentAmount(requirement) >= requirement.amount;
    }

    public static bool TryPay(CageRequirement requirement)
    {
        if (!CanAfford(requirement))
            return false;

        switch (requirement.type)
        {
            case CageRequirementType.Coin:
                return PlayerStats.Instance != null
                    && PlayerStats.Instance.TrySpendCoins(requirement.amount);

            case CageRequirementType.Item:
                return InventoryManager.Instance != null
                    && InventoryManager.Instance.TryRemoveItems(requirement.ItemID, requirement.amount);

            default:
                return false;
        }
    }

    public static string GetRequirementLabel(CageRequirement requirement)
    {
        if (requirement == null)
            return "vật phẩm";

        if (requirement.type == CageRequirementType.Coin)
            return "coin";

        if (requirement.itemData != null && !string.IsNullOrEmpty(requirement.itemData.itemName))
            return requirement.itemData.itemName;

        return "vật phẩm";
    }
}
