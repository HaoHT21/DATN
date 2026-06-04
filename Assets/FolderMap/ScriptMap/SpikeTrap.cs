using UnityEngine;

public class TrapData : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;

    [Header("Animated Tile")]
    public float animationSpeed = 1f; // giống Min/Max Speed của Animated Tile
    public int totalFrames = 4;
    public int damageFrame = 3; // Sprite cuối (bắt đầu từ 0)

    private bool canDamage;

    void Update()
    {
        // Tính frame hiện tại của Animated Tile
        int currentFrame =
            Mathf.FloorToInt(Time.time * animationSpeed) % totalFrames;

        canDamage = (currentFrame == damageFrame);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();

            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
        }
    }
}