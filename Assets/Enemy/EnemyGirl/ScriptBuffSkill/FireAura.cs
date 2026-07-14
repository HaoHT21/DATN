using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class FireAura : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;

    public float damageInterval = 0.5f;

    [Header("Life")]
    public float lifeTime = 5f;

    private Transform owner;

    private readonly List<GameObject> targets =
        new List<GameObject>();

    //--------------------------------------

    public void SetOwner(Transform player)
    {
        owner = player;
    }

    //--------------------------------------

    void Start()
    {
        Destroy(gameObject, lifeTime);

        InvokeRepeating(
            nameof(DamageTargets),
            damageInterval,
            damageInterval
        );
    }

    //--------------------------------------

    void Update()
    {
        if (owner != null)
        {
            transform.position = owner.position;
        }
    }

    //--------------------------------------

    void DamageTargets()
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null)
            {
                targets.RemoveAt(i);
                continue;
            }

            EnemyHealth enemy =
                targets[i].GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            BossHeath boss =
                targets[i].GetComponent<BossHeath>();

            if (boss != null)
            {
                boss.TakeDamage(damage);
            }
        }
    }

    //--------------------------------------

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") ||
            other.CompareTag("Boss"))
        {
            if (!targets.Contains(other.gameObject))
                targets.Add(other.gameObject);
        }
    }

    //--------------------------------------

    void OnTriggerExit2D(Collider2D other)
    {
        targets.Remove(other.gameObject);
    }
}