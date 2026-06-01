using UnityEngine;

public enum HealthBarFillMode
{
    Slider,
    ImageFill
}

public enum HealthBarTextMode
{
    None,
    CurrentOverMax
}

[System.Serializable]
public class HealthBarSmoothSettings
{
    [Tooltip("Bật để thanh giảm/tăng mượt thay vì nhảy tức thì.")]
    public bool enabled = true;

    [Tooltip("Tốc độ Lerp (càng cao càng nhanh đuổi kịp giá trị thật).")]
    [Min(0.01f)]
    public float smoothSpeed = 8f;

    [Tooltip("Thanh đuổi theo (damage trail) — tùy chọn, để trống nếu không dùng.")]
    public bool useDelayBar;
}

[System.Serializable]
public class HealthBarBlinkSettings
{
    public bool enabled = true;
    public Color damageFlashColor = new Color(1f, 0.25f, 0.25f, 1f);
    public int flashCount = 3;
    [Min(0.02f)] public float flashInterval = 0.08f;
}
