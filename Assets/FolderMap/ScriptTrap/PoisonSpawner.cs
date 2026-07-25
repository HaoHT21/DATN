using UnityEngine;

public class PoisonSpawner : MonoBehaviour
{
    [Header("Health")]
    public int hp = 1;

    [Header("Poison")]
    public GameObject poisonAreaPrefab;

    private bool activated = false;

    public void TakeDamage(int damage)
    {
        if (activated) return;

        hp -= damage;

        if (hp <= 0)
        {
            SpawnPoison();
        }
    }

    private void SpawnPoison()
    {
        activated = true;

        if (poisonAreaPrefab != null)
        {
            Instantiate(
                poisonAreaPrefab,
                transform.position,
                Quaternion.identity);
        }

        Destroy(gameObject);
    }
}