using UnityEngine;

public class BossFireball : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 20;

    private void Start() => Destroy(gameObject, 3f); // Tự hủy sau 3s

    private void Update() => transform.Translate(Vector3.right * speed * Time.deltaTime);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}