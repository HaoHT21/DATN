using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    public GameObject coinPrefab;

    [Header("Coin Drop")]
    public int minCoin = 1;
    public int maxCoin = 5;

    private EnemyHeath enemyHealth;
    private BossHeath bossHealth;

    private bool isBoss = false;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHeath>();
        bossHealth = GetComponent<BossHeath>();

        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += DropCoins;
            isBoss = false;
        }

        if (bossHealth != null)
        {
            bossHealth.OnDeath += DropCoins;
            isBoss = true;
        }
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

        for (int i = 0; i < coinAmount; i++)
        {
            Vector3 pos = transform.position +
                          (Vector3)(Random.insideUnitCircle * 0.5f);

            GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity);

            Coin coinScript = coin.GetComponent<Coin>();
            if (coinScript != null)
            {
                // Enemy -> tự hủy sau 10 giây
                // Boss -> không tự hủy
                coinScript.SetAutoDestroy(!isBoss, 10f);
            }
        }
    }
}