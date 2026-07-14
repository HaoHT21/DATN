using UnityEngine;

public class KillRandomEnemySkill : NPCSkill
{
    [Header("Damage")]
    public int damage = 999999;

    public override void Use(GameObject player)
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
            return;

        GameObject target =
            enemies[Random.Range(0, enemies.Length)];

        EnemyHealth enemy =
            target.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}