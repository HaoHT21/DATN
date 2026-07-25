using UnityEngine;

public class IceSlide : MonoBehaviour
{
    [Header("Slide")]
    public float slideSpeed = 8f;

    [Header("Auto Stop")]
    public float maxSlideTime = 1f;

    private PlayerController player;
    private Rigidbody2D rb;

    private bool sliding;
    private Vector2 slideDirection;

    private float slideTimer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        player = other.GetComponent<PlayerController>();
        rb = other.GetComponent<Rigidbody2D>();

        if (player == null || rb == null)
            return;

        slideDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (slideDirection == Vector2.zero)
            return;

        sliding = true;
        slideTimer = maxSlideTime;

        player.enabled = false;
    }

    private void FixedUpdate()
    {
        if (!sliding || rb == null)
            return;

        rb.linearVelocity = slideDirection * slideSpeed;

        slideTimer -= Time.fixedDeltaTime;

        if (slideTimer <= 0f)
            StopSlide();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        StopSlide();
    }

    private void StopSlide()
    {
        if (!sliding)
            return;

        sliding = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (player != null)
            player.enabled = true;
    }
}