using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class TrapController : MonoBehaviour
{
    [Header("Trap Settings")]
    public TilemapCollider2D trapCollider;

    [Header("Damage")]
    public int damage = 10;

    [Tooltip("Thời gian trap tắt")]
    public float inactiveTime = 1f;

    [Tooltip("Thời gian trap bật")]
    public float activeTime = 1f;

    private void Start()
    {
        if (trapCollider != null)
        {
            trapCollider.isTrigger = true;
            trapCollider.enabled = false;
        }

        StartCoroutine(TrapRoutine());
    }

    IEnumerator TrapRoutine()
    {
        while (true)
        {
            // Trap tắt
            trapCollider.enabled = false;
            yield return new WaitForSeconds(inactiveTime);

            // Trap bật
            trapCollider.enabled = true;
            yield return new WaitForSeconds(activeTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!trapCollider.enabled)
            return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth health =
                other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}