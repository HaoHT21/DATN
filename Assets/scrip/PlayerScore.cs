using Fusion;
using TMPro;
using UnityEngine;

public class PlayerScore : NetworkBehaviour
{
    [Networked]
    public int Score { get; set; }

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    private int oldScore;

    public override void Spawned()
    {
        // Tự tìm UI tên CoinScore
        GameObject scoreObj =
            GameObject.Find("CoinScore");

        if (scoreObj != null)
        {
            scoreText =
                scoreObj.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError(
                "Không tìm thấy CoinScore"
            );
        }

        UpdateUI();
    }

    void Update()
    {
        // Chỉ player local update UI
        if (!Object.HasInputAuthority)
            return;

        // Nếu score đổi thì update
        if (oldScore != Score)
        {
            oldScore = Score;
            UpdateUI();
        }
    }

    public void AddCoin(int amount)
    {
        // Chỉ host/server cộng điểm
        if (Object.HasStateAuthority)
        {
            Score += amount;
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "" + Score;
        }
    }
}