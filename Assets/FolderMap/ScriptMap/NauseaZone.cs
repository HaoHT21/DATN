using UnityEngine;

public class NauseaZone : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private SpriteRenderer background;

    [SerializeField] private Color zoneColor = Color.red;

    private Color defaultColor;

    private void Awake()
    {
        if (background != null)
            defaultColor = background.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        background.color = zoneColor;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        background.color = defaultColor;
    }
}