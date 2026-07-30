using UnityEngine;

public enum EffectType
{
    Freeze,   // Đóng băng
    Poison,   // Trúng độc
    Slow,     // Làm chậm
    Burn      // Bỏng
}

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Game/Status Effect")]
public class StatusEffectSO : ScriptableObject
{
    public EffectType effectType;      // Loại hiệu ứng
    public string effectName;          // Tên hiệu ứng
    public float duration = 5f;        // Thời gian tác dụng
    public GameObject visualPrefab;    // Prefab particle/visual gắn lên Player
}