using System.Collections;
using UnityEngine;

public class ShieldSkill : NPCSkill
{
    [Header("Effect Data")]
    [Tooltip("Kéo ScriptableObject Khiên (SO_Shield) vào đây")]
    public StatusEffectSO shieldEffectSO;

    public override void Use(GameObject player)
    {
        if (player.TryGetComponent<EffectManager>(out EffectManager effectManager))
        {
            if (shieldEffectSO != null)
            {
                // Áp dụng Effect Khiên với thời gian duration định sẵn từ Skill
                effectManager.ApplyEffect(shieldEffectSO, duration);
            }
        }

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