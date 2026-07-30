using UnityEngine;

public class SummonEnemySkill : NPCSkill
{
    [Header("Summon")]
    public GameObject summonPrefab;

    public int summonCount = 3;

    public float spawnRadius = 2f;

    public override void Use(GameObject player)
    {
        if (summonPrefab == null)
            return;

        for (int i = 0; i < summonCount; i++)
        {
            Vector2 randomPos =
                (Vector2)player.transform.position +
                Random.insideUnitCircle * spawnRadius;

            Instantiate(
                summonPrefab,
                randomPos,
                Quaternion.identity
            );
        }
    }
}