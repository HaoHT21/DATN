using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    public GameObject coinPrefab;
    public int coinAmount = 5;

    private EnemyHeath health;

    private void Awake()
    {
        health = GetComponent<EnemyHeath>();

        if (health != null)
            health.OnDeath += DropCoins;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= DropCoins;
    }

    private void DropCoins()
    {
        Debug.Log("DROP COINS");

        for (int i = 0; i < coinAmount; i++)
        {
            Vector3 pos = transform.position +
                          (Vector3)(Random.insideUnitCircle * 0.5f);

            Instantiate(coinPrefab, pos, Quaternion.identity);
        }
    }
}