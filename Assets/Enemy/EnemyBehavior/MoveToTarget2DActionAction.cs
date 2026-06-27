using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToTarget2DAction ", story: "MoveToTarget2DAction", category: "Action", id: "c5bf07e3c931fe3880b5a3f4dbd54927")]
public partial class MoveToTarget2DActionAction : Action
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Self;

    [SerializeReference]
    public BlackboardVariable<GameObject> Target;

    [SerializeReference]
    public BlackboardVariable<float> Speed;

    [SerializeReference]
    public BlackboardVariable<float> AttackRange;

    [SerializeReference]
    public BlackboardVariable<float> DashSpeed;

    [SerializeReference]
    public BlackboardVariable<float> DashDuration;

    [SerializeReference]
    public BlackboardVariable<float> AttackWindupTime;

    [SerializeReference]
    public BlackboardVariable<GameObject> AttackHitbox;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isAttacking;
    private float attackTimer;
    private Vector2 dashDirection;

    private bool isPreparingAttack;
    private Vector2 lockedAttackPosition;



    protected override Status OnStart()
    {
        Debug.Log("Hitbox = " + AttackHitbox?.Value);
        if (Self.Value == null || Target.Value == null)
            return Status.Failure;

        rb = Self.Value.GetComponent<Rigidbody2D>();
        animator = Self.Value.GetComponent<Animator>();
        spriteRenderer = Self.Value.GetComponent<SpriteRenderer>();

        if (rb == null)
            return Status.Failure;

        if (animator != null)
            animator.Play("idle");

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (isPreparingAttack)
        {
            attackTimer -= Time.deltaTime;

            rb.linearVelocity = Vector2.zero;

            if (attackTimer <= 0)
            {
                isPreparingAttack = false;

                dashDirection =
                    (lockedAttackPosition -
                     (Vector2)Self.Value.transform.position).normalized;

                isAttacking = true;
                attackTimer = DashDuration.Value;
            }

            return Status.Running;
        }

        if (Target.Value == null)
            return Status.Failure;

        float distance = Vector2.Distance(
            Self.Value.transform.position,
            Target.Value.transform.position);

        // Đang dash attack
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            rb.linearVelocity = dashDirection * DashSpeed.Value;

            if (AttackHitbox.Value != null)
                AttackHitbox.Value.SetActive(true);
            if (AttackHitbox.Value == null)
            {
                Debug.LogError("AttackHitbox NULL");
            }

            if (attackTimer <= 0)
            {
                isAttacking = false;

                if (AttackHitbox.Value != null)
                    AttackHitbox.Value.SetActive(false);

                if (animator != null)
                    animator.Play("idle");
            }

            return Status.Running;
        }

        // Vào tầm đánh
        if (distance <= AttackRange.Value)
        {
            isPreparingAttack = true;

            attackTimer = AttackWindupTime.Value;

            lockedAttackPosition = Target.Value.transform.position;

            if (animator != null)
                animator.Play("attack");

            return Status.Running;
        }

        // Chase
        Vector2 direction =
            ((Vector2)Target.Value.transform.position -
             (Vector2)Self.Value.transform.position).normalized;

        rb.linearVelocity = direction * Speed.Value;

        if (spriteRenderer != null)
        {
            if (direction.x > 0.05f)
                spriteRenderer.flipX = false;
            else if (direction.x < -0.05f)
                spriteRenderer.flipX = true;
        }

        if (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            if (!state.IsName("idle"))
                animator.Play("idle");
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        isAttacking = false;
    }
}
