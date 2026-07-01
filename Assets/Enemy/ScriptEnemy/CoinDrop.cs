using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    public GameObject coinPrefab;

    [Header("Coin Drop")]
    public int minCoin = 1;
    public int maxCoin = 5;

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
        int coinAmount = Random.Range(minCoin, maxCoin + 1);

        Debug.Log($"DROP {coinAmount} COINS");

        for (int i = 0; i < coinAmount; i++)
        {
            Vector3 pos = transform.position +
                          (Vector3)(Random.insideUnitCircle * 0.5f);

            Instantiate(coinPrefab, pos, Quaternion.identity);
        }
    }
}