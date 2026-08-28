using UnityEngine;

/// <summary>
/// Lưu / tải dữ liệu phiên chơi tối thiểu khi thoát màn hoặc thoát ứng dụng.
/// Đã được đồng bộ với SaveManager và SavesData.
/// </summary>
public static class GameSessionSave
{
    public static void SaveCurrentSession()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogWarning("[GameSessionSave] Không tìm thấy SaveManager Instance trong Scene!");
        }
    }

    public static void LoadInto(PlayerStats stats)
    {
        if (stats == null)
            return;

        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            // Tải lại toàn bộ dữ liệu game (gồm Vị trí, Hero, Máu, Coins, Inventory...)
            SaveManager.Instance.LoadGame();
        }
        else
        {
            Debug.LogWarning("[GameSessionSave] Không thể Load vì thiếu SaveManager hoặc chưa có file Save!");
        }
    }
}