using System.Collections;
using UnityEngine;

public class HoleTrap : MonoBehaviour
{
    [Header("Animation")]
    public float pullTime = 0.6f;
    public float rotateSpeed = 900f;
    public int damage = 20;

    [Header("Respawn")]
    public float safeDistance = 0.6f;
    public float immunityTime = 1f;

    [Header("Delay")]
    public float triggerDelay = 0.3f;

    private bool usingTrap;

    private static Vector3 lastSafePosition;

    private void Start()
    {
        lastSafePosition = transform.position;
    }

    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        if (!usingTrap)
        {
            if (Vector2.Distance(player.transform.position, transform.position) > safeDistance)
            {
                lastSafePosition = player.transform.position;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (usingTrap)
            return;

        if (!other.CompareTag("Player"))
            return;

        StartCoroutine(DelayTrigger(other.gameObject));
    }

    IEnumerator DelayTrigger(GameObject player)
    {
        usingTrap = true;

        yield return new WaitForSeconds(triggerDelay);

        // Nếu Player đã bị hủy
        if (player == null)
        {
            usingTrap = false;
            yield break;
        }

        BoxCollider2D box = player.GetComponent<BoxCollider2D>();
        Collider2D trapCol = GetComponent<Collider2D>();

        if (box == null || trapCol == null || !box.IsTouching(trapCol))
        {
            usingTrap = false;
            yield break;
        }

        yield return StartCoroutine(FallRoutine(player));
    }

    IEnumerator FallRoutine(GameObject player)
    {
        usingTrap = true;

        PlayerController controller = player.GetComponent<PlayerController>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        BoxCollider2D box = player.GetComponent<BoxCollider2D>();

        // Tắt điều khiển
        controller.enabled = false;
        rb.linearVelocity = Vector2.zero;

        // Tắt BoxCollider ngay khi rơi xuống hố
        if (box != null)
            box.enabled = false;

        Vector3 startScale = player.transform.localScale;
        Quaternion startRotation = player.transform.rotation;

        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / pullTime;

            player.transform.localScale =
                Vector3.Lerp(startScale, Vector3.zero, t);

            player.transform.Rotate(
                0,
                0,
                rotateSpeed * Time.deltaTime);

            yield return null;
        }

        // Gây sát thương
        if (health != null)
            health.TakeDamage(damage);

        // Khôi phục hình dạng
        player.transform.localScale = startScale;
        player.transform.rotation = startRotation;

        // Bật lại điều khiển
        controller.enabled = true;

        // Chờ miễn nhiễm
        yield return new WaitForSeconds(immunityTime);

        // Bật lại BoxCollider
        if (box != null)
            box.enabled = true;

        usingTrap = false;
    }
}