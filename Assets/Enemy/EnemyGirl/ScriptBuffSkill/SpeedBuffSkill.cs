using UnityEngine;

public class SpeedBuffSkill : NPCSkill
{
    public float bonusSpeed = 2f;

    public override void Use(GameObject player)
    {
        EffectManager effect =
            player.GetComponent<EffectManager>();

        if (effect != null)
        {
            effect.AddSpeedTemporary(
                bonusSpeed,
                duration
            );
        }
    }
}