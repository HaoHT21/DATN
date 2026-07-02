using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    public GameObject coinPrefab;

    [Header("Coin Drop")]
    public int minCoin = 1;
    public int maxCoin = 5;

    private EnemyHeath enemyHealth;
    private BossHeath bossHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHeath>();
        bossHealth = GetComponent<BossHeath>();

        if (enemyHealth != null)
            enemyHealth.OnDeath += DropCoins;

        if (bossHealth != null)
            bossHealth.OnDeath += DropCoins;
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
            enemyHealth.OnDeath -= DropCoins;

        if (bossHealth != null)
            bossHealth.OnDeath -= DropCoins;
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