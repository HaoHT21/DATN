using Fusion;
using UnityEngine;

public class CoinMagnet : NetworkBehaviour
{
    [Header("Khoảng hút")]
    public float detectRange = 3f;

    [Header("Tốc độ bay")]
    public float moveSpeed = 10f;

    private Transform targetPlayer;
    private bool isFlying = false;

    void Update()
    {
        // Nếu chưa bay -> tìm player gần nhất
        if (!isFlying)
        {
            FindPlayer();
        }
        else
        {
            FlyToPlayer();
        }
    }

    void FindPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = detectRange;

        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(
                transform.position,
                player.transform.position
            );

            if (distance <= closestDistance)
            {
                closestDistance = distance;
                targetPlayer = player.transform;
                isFlying = true;
            }
        }
    }

    void FlyToPlayer()
    {
        if (targetPlayer == null)
            return;

        // Coin bay tới player
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPlayer.position,
            moveSpeed * Time.deltaTime
        );

        // Khi chạm player
        float distance = Vector2.Distance(
            transform.position,
            targetPlayer.position
        );

        if (distance < 0.2f)
        {
            PlayerScore score = targetPlayer.GetComponent<PlayerScore>();

            if (score != null)
            {
                score.AddCoin(1);
            }

            // Host xoá coin
            if (Object.HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }
    }

}