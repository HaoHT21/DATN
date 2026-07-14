using UnityEngine;

public class HealSkill : NPCSkill
{
    [Range(0f, 1f)]
    public float healPercent = 0.25f;

    public override void Use(GameObject player)
    {
        PlayerHealth health =
            player.GetComponent<PlayerHealth>();

        if (health == null)
            return;

        int heal =
            Mathf.RoundToInt(
                health.MaxHealth * healPercent);

        health.Heal(heal);
    }
}