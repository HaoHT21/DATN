using UnityEngine;

/// <summary>
/// Gắn SaveableEntity + CollectibleSaveable runtime với id ổn định giữa các lần load.
/// </summary>
public static class SaveableHelpers
{
    public static void EnsureCollectible(GameObject go, string stableId)
    {
        if (go == null)
            return;

        SaveableEntity entity = go.GetComponent<SaveableEntity>();
        if (entity == null)
            entity = go.AddComponent<SaveableEntity>();

        entity.EnsureId(stableId, forceOverwrite: string.IsNullOrWhiteSpace(entity.Id));

        if (go.GetComponent<CollectibleSaveable>() == null)
            go.AddComponent<CollectibleSaveable>();
    }
}
