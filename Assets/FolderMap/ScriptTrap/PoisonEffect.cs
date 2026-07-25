using UnityEngine;
using System.Collections;

public class PoisonEffect : MonoBehaviour
{
    private Coroutine poisonRoutine;

    public void ApplyPoison(int damage, float interval, float duration)
    {
        if (poisonRoutine != null)
            StopCoroutine(poisonRoutine);

        poisonRoutine = StartCoroutine(PoisonRoutine(damage, interval, duration));
    }

    private IEnumerator PoisonRoutine(int damage, float interval, float duration)
    {
        float timer = duration;
        PlayerHealth player = GetComponent<PlayerHealth>();

        while (timer > 0)
        {
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            yield return new WaitForSeconds(interval);
            timer -= interval;
        }

        // Hết thời gian độc -> Tự xóa component này khỏi Player để sạch sẽ
        Destroy(this);
    }
}