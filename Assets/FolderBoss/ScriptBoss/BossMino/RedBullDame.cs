using UnityEngine;

public class RedBullDame : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (
            !other.CompareTag(
                "Player"
            )
        )
            return;

        Debug.Log(
            "Laser Hit Player"
        );

        PlayerHealth player =
            other.GetComponent<
                PlayerHealth>();

        if (player != null)
        {
            Vector2 hitDirection =
            (
                other.transform.position
                - transform.position
            ).normalized;

            player.TakeDamage(
                damage,
                hitDirection
            );
        }
    }
}