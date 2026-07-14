using UnityEngine;

public class FireAuraSkill : NPCSkill
{
    [Header("Prefab")]
    public GameObject fireAuraPrefab;

    public override void Use(GameObject player)
    {
        if (fireAuraPrefab == null)
            return;

        GameObject aura = Instantiate(
            fireAuraPrefab,
            player.transform.position,
            Quaternion.identity
        );

        FireAura area =
            aura.GetComponent<FireAura>();

        if (area != null)
        {
            area.SetOwner(player.transform);
            area.lifeTime = duration;
        }
    }
}