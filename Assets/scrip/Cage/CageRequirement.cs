using System;
using UnityEngine;

public enum CageRequirementType
{
    Coin,
    Item
}

[Serializable]
public class CageRequirement
{
    public CageRequirementType type = CageRequirementType.Coin;
    public int amount = 500;
    public ItemData itemData;

    public int ItemID => itemData != null ? itemData.itemID : 0;
}
