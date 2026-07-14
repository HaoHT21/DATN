using UnityEngine;

public class BulletRainSkill : NPCSkill
{
    public GameObject rainAreaPrefab;

    public override void Use(GameObject player)
    {
        if (rainAreaPrefab == null)
            return;

        GameObject obj = Instantiate(
            rainAreaPrefab,
            player.transform.position,
            Quaternion.identity);

        BulletRainArea area =
            obj.GetComponent<BulletRainArea>();

        if (area != null)
            area.Initialize(player, duration);
    }
}