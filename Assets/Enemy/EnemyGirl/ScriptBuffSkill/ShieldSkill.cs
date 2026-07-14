using System.Collections;
using UnityEngine;

public class ShieldSkill : NPCSkill
{
    public override void Use(GameObject player)
    {
        PlayerHealth health =
            player.GetComponent<PlayerHealth>();

        if (health == null)
            return;

        health.StartCoroutine(
            ShieldRoutine(health)
        );
    }

    IEnumerator ShieldRoutine(PlayerHealth health)
    {
        health.SetInvincible(true);

        yield return new WaitForSeconds(duration);

        health.SetInvincible(false);
    }
}