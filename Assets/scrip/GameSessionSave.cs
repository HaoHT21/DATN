using UnityEngine;

/// <summary>
/// Lưu / tải dữ liệu phiên chơi tối thiểu khi thoát màn hoặc thoát ứng dụng.
/// Mở rộng thêm key PlayerPrefs hoặc file save tại đây khi dự án phát triển.
/// </summary>
public static class GameSessionSave
{
    private const string ScoreKey = "PlayerScore";

    public static void SaveCurrentSession()
    {
        PlayerStats stats = Object.FindFirstObjectByType<PlayerStats>();
        if (stats != null)
            PlayerPrefs.SetInt(ScoreKey, stats.Score);

        PlayerPrefs.Save();
        Debug.Log("[GameSessionSave] Đã lưu dữ liệu phiên chơi.");
    }

    public static void LoadInto(PlayerStats stats)
    {
        if (stats == null || !PlayerPrefs.HasKey(ScoreKey))
            return;

        stats.Score = PlayerPrefs.GetInt(ScoreKey);
        stats.UpdateUI();
    }
}
