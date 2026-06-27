using UnityEngine;

public class BattleResetZone : MonoBehaviour
{
    public Barrier barrier;

    private int playerCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (barrier.IsBattleCompleted())
            return;

        if (other.CompareTag("Player"))
        {
            playerCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (barrier.IsBattleCompleted())
            return;

        if (other.CompareTag("Player"))
        {
            playerCount--;

            if (playerCount <= 0)
            {
                barrier.ResetBattle();
            }
        }
    }
}