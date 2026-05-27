using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    [HideInInspector] public int damage;

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<EnemyHealth>(out var e))
        {
            e.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}