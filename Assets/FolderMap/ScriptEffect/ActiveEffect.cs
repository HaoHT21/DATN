using UnityEngine;

[System.Serializable]
public class ActiveEffect
{
    public StatusEffectSO data;
    public float remainingTime;
    public GameObject visualInstance;

    public ActiveEffect(StatusEffectSO data, GameObject visualInstance, float duration)
    {
        this.data = data;
        this.visualInstance = visualInstance;
        this.remainingTime = duration;
    }
}