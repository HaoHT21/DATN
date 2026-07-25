using UnityEngine;
using System.Collections;

public class SwordWeapon : WeaponBase
{
    public Animator animator;
    public PolygonCollider2D hitbox;

    [Header("Animation")]
    public string attackAnimationName = "Sword1";
    public string idleAnimationName = "Idle";

    [Header("Attack")]
    public float attackDuration = 0.2f;
    public float activeHitboxTime = 0.15f;

    private bool isAttacking;

    // Không tắt collider trong Awake nữa
    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (hitbox == null)
            hitbox = GetComponentInChildren<PolygonCollider2D>();
    }

    // Chỉ khi Player nhặt mới tắt hitbox
    public override void OnEquip()
    {
        if (hitbox != null)
            hitbox.enabled = false;
    }

    public override void Attack()
    {
        if (isAttacking)
            return;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        Debug.Log("Animator = " + animator);
        Debug.Log("Attack State = " + attackAnimationName);

        if (animator != null)
            animator.Play(attackAnimationName, 0, 0);

        if (hitbox != null)
            hitbox.enabled = true;

        yield return new WaitForSeconds(activeHitboxTime);

        Debug.Log(animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimationName));

        if (hitbox != null)
            hitbox.enabled = false;

        float remain = attackDuration - activeHitboxTime;

        if (remain > 0)
            yield return new WaitForSeconds(remain);

        if (animator != null)
            animator.Play(idleAnimationName, 0, 0);

        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking)
            return;

        if (other.CompareTag("BulletEnemy"))
        {
            Destroy(other.gameObject);
            return;
        }

        DamageEnemy(other);
    }
}