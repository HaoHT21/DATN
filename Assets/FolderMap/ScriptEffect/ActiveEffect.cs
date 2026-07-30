using UnityEngine;

public class ActiveEffect
{
    public StatusEffectSO data;
    public float timer;
    public GameObject visualInstance; // Prefab đang gắn trên người Player

    public ActiveEffect(StatusEffectSO data, GameObject visualInstance)
    {
        this.data = data;
        this.timer = data.duration;
        this.visualInstance = visualInstance;
    }
}