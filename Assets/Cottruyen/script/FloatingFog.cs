using System.Collections.Generic;
using UnityEngine;

public class FloatingFog : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject fogPrefab;

    [Header("Area")]
    public int amount = 20;
    public float spacing = 1.2f;

    [Header("Movement")]
    public float moveSpeed = 0.15f;
    public float moveRange = 1f;

    [Header("Random Offset")]
    public float randomY = 0.2f;

    private List<Transform> fogs = new List<Transform>();
    private Vector3[] startPos;

    void Start()
    {
        startPos = new Vector3[amount];

        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = transform.position;

            pos.x += i * spacing;
            pos.y += Random.Range(-randomY, randomY);

            GameObject fog = Instantiate(fogPrefab, pos, Quaternion.identity, transform);

            fogs.Add(fog.transform);
            startPos[i] = fog.transform.localPosition;
        }
    }

    void Update()
    {
        for (int i = 0; i < fogs.Count; i++)
        {
            Vector3 p = startPos[i];

            p.x += Mathf.Sin(Time.time * moveSpeed + i) * moveRange;

            fogs[i].localPosition = p;
        }
    }
}