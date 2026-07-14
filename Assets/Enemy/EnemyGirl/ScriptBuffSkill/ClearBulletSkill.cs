using UnityEngine;

public class ClearBulletSkill : NPCSkill
{
    [Header("Prefab")]
    public GameObject clearWavePrefab;

    public override void Use(GameObject player)
    {
        if (clearWavePrefab == null)
            return;

        Instantiate(
            clearWavePrefab,
            player.transform.position,
            Quaternion.identity
        );
    }
}