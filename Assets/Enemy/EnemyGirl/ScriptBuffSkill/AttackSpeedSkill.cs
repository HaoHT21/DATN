using UnityEngine;

public class AttackSpeedSkill : NPCSkill
{
    public float attackSpeed = 0.5f;

    public override void Use(GameObject player)
    {
        EffectManager effect =
            player.GetComponent<EffectManager>();

        if (effect != null)
        {
            effect.AddAttackSpeedTemporary(
                attackSpeed,
                duration);
        }
    }
}