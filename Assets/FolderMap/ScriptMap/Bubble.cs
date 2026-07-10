using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float lifeTime = 8f;

    private SwimZone zone;

    private void Start()
    {
        zone = FindFirstObjectByType<SwimZone>();

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (zone != null)
            zone.SetInsideBubble(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (zone != null)
            zone.SetInsideBubble(false);
    }

    private void OnDestroy()
    {
        if (zone != null)
            zone.SetInsideBubble(false);
    }
}