using UnityEngine;

public class MeleeDame : MonoBehaviour
{
    public int damage = 20;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth player =
            other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }
}