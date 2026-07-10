using UnityEngine;

public class FogFloat : MonoBehaviour
{
    public float floatHeight = 0.1f;
    public float floatSpeed = 1f;

    private Vector3 startPos;
    private float randomOffset;

    void Start()
    {
        startPos = transform.localPosition;
        randomOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        Vector3 p = startPos;

        p.y += Mathf.Sin(Time.time * floatSpeed + randomOffset) * floatHeight;

        transform.localPosition = p;
    }
}