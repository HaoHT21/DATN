using UnityEngine;

public enum EffectType
{
    Freeze,   // Đóng băng
    Poison,   // Trúng độc
    Slow,     // Làm chậm
    Burn,      // Bỏng

    Shield // Khiên (miễn nhiễm sát thương)
}

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Game/Status Effect")]
public class StatusEffectSO : ScriptableObject
{
    public EffectType effectType;      // Loại hiệu ứng
    public string effectName;          // Tên hiệu ứng
    public float baseDuration = 3f;    // Thời gian tác dụng mặc định (giây)
    public GameObject visualPrefab;    // Prefab particle/visual gắn lên Player
}