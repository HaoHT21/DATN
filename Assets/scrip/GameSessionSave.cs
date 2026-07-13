using UnityEngine;

/// <summary>
/// Lưu / tải dữ liệu phiên chơi tối thiểu khi thoát màn hoặc thoát ứng dụng.
/// Mở rộng thêm key PlayerPrefs hoặc file save tại đây khi dự án phát triển.
/// </summary>
public static class GameSessionSave
{
    public static void SaveCurrentSession()
    {
        SaveGameService.Save(SaveGameService.CaptureFromCurrentScene());
    }

    public static void LoadInto(PlayerStats stats)
    {
        if (stats == null)
            return;

        SaveData data = SaveGameService.Load();
        if (data == null)
            return;

        stats.Score = data.score;
        stats.UpdateUI();
    }
}
