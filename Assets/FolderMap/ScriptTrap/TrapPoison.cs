using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrapPoison1 : MonoBehaviour
{
    [Header("Trap Setting")]
    public TilemapCollider2D trapPoisonCollider;

    public float inactiveTime = 1f;
    public float outactiveTime = 1f;

    [Header("Thông số sát thương độc")]
    public int poisonDamage = 2;
    public float damageInterval = 1f;

    [Header("Hiệu ứng sau khi rời bẫy")]
    [Tooltip("Thời gian độc còn tồn tại trên người sau khi chạy thoát khỏi bẫy")]
    public float poisonDurationAfterExit = 3f;

    private void Start()
    {
        if (trapPoisonCollider != null)
        {
            trapPoisonCollider.isTrigger = true;
            trapPoisonCollider.enabled = false; 
        }

        StartCoroutine(TrapPoisonRoutine());
    }

    IEnumerator TrapPoisonRoutine()
    {
        while (true)
        {
            trapPoisonCollider.enabled = false;
            yield return new WaitForSeconds(inactiveTime);

            trapPoisonCollider.enabled = true;
            yield return new WaitForSeconds(outactiveTime);
        }
    }

    private void HandlePoison(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<PlayerHealth>(out PlayerHealth player))
        {
            PoisonEffect poison = player.GetComponent<PoisonEffect>();

            if (poison == null)
            {
                poison = player.gameObject.AddComponent<PoisonEffect>();
            }

            poison.ApplyPoison(poisonDamage, damageInterval, poisonDurationAfterExit);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePoison(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandlePoison(other);
    }
}