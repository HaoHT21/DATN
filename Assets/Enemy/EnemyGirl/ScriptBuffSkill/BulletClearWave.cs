using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class BulletClearWave : MonoBehaviour
{
    [Header("Wave")]
    public float maxRadius = 5f;

    public float expandTime = 0.4f;

    public float destroyDelay = 0.2f;

    [Header("Layer")]
    public LayerMask bulletLayer;

    private CircleCollider2D circle;

    void Awake()
    {
        circle = GetComponent<CircleCollider2D>();

        circle.isTrigger = true;

        transform.localScale = Vector3.zero;
    }

    void Start()
    {
        StartCoroutine(Expand());
    }

    System.Collections.IEnumerator Expand()
    {
        float timer = 0;

        while (timer < expandTime)
        {
            timer += Time.deltaTime;

            float t = timer / expandTime;

            float scale = Mathf.Lerp(0, maxRadius, t);

            transform.localScale =
                Vector3.one * scale;

            yield return null;
        }

        transform.localScale =
            Vector3.one * maxRadius;

        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & bulletLayer) != 0)
        {
            Destroy(other.gameObject);
        }
    }
}