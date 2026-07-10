using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 1.5f;

    public float attackWindupTime = 0.3f;

    public float dashSpeed = 10f;

    public float dashDuration = 0.3f;

    [Header("Recovery")]
    public float recoveryTime = 1f;

    [Header("Hitbox")]
    public GameObject attackHitbox;


    private EnemyController controller;
    private Rigidbody2D rb;
    private Animator animator;

    private bool isPreparingAttack;
    private bool isAttacking;

    private float attackTimer;

    private Vector2 dashDirection;
    private Vector2 lockedAttackPosition;

    private string currentAnim;


    private bool isRecovering;
    private float recoveryTimer;


    private void Awake()
    {
        controller =
            GetComponent<EnemyController>();

        rb =
            GetComponent<Rigidbody2D>();

        animator =
            GetComponent<Animator>();

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(
                false);
        }
    }


    private void Update()
    {
        //--------------------------------
        // Mất target -> hủy attack
        //--------------------------------

        if (!controller.HasTarget)
        {
            if (
                isPreparingAttack ||
                isAttacking ||
                isRecovering
            )
            {
                CancelAttack();
            }

            return;
        }

        //--------------------------------
        // Hurt -> hủy attack
        //--------------------------------

        if (controller.IsHurting)
        {
            return;
        }

        Transform target =
            controller.Target;

        float distance =
            Vector2.Distance(
                transform.position,
                target.position
            );

        //--------------------------------
        // Nghỉ sau dash
        //--------------------------------

        if (isRecovering)
        {
            recoveryTimer -=
                Time.deltaTime;

            controller.StopMovement();

            controller.PlayAnimation(
                "idle"
            );

            //--------------------------------
            // Mất player khi đang nghỉ
            //--------------------------------

            if (!controller.HasTarget)
            {
                isRecovering =
                    false;

                controller.LockMovement(
                    false
                );

                return;
            }

            //--------------------------------
            // Hết thời gian nghỉ
            //--------------------------------

            if (
                recoveryTimer <= 0
            )
            {
                isRecovering =
                    false;

                controller.LockMovement(
                    false
                );
            }

            return;
        }


        //--------------------------------
        // WINDUP
        //--------------------------------

        if (isPreparingAttack)
        {
            attackTimer -=
                Time.deltaTime;

            controller.StopMovement();

            controller.LookAt(
                lockedAttackPosition
            );

            if (attackTimer <= 0)
            {
                isPreparingAttack =
                    false;

                isAttacking =
                    true;

                attackTimer =
                    dashDuration;

                dashDirection =
                    (
                        lockedAttackPosition -
                        (Vector2)
                        transform.position
                    ).normalized;
            }

            return;
        }


        //--------------------------------
        // DASH
        //--------------------------------

        if (isAttacking)
        {
            attackTimer -=
                Time.deltaTime;

            rb.linearVelocity =
                dashDirection *
                dashSpeed;

            if (attackHitbox != null)
            {
                attackHitbox.SetActive(
                    true);
            }

            if (attackTimer <= 0)
            {
                StartRecovery();
            }

            return;
        }


        //--------------------------------
        // Vào vùng đỏ
        //--------------------------------

        if (
            distance <=
            attackRange
            &&
            !isPreparingAttack
            &&
            !isAttacking
            &&
            !isRecovering
        )
        {
            isPreparingAttack =
                true;

            attackTimer =
                attackWindupTime;

            lockedAttackPosition =
                target.position;

            controller.LockMovement(
                true
            );

            controller.StopMovement();

            controller.LookAt(
                lockedAttackPosition
            );

            controller.PlayAnimation(
                "attack"
            );
        }
        else
        {
            controller.LockMovement(
                false
            );
        }
    }


    void CancelAttack()
    {
        isPreparingAttack =
            false;

        isAttacking =
            false;

        isRecovering =
            false;

        attackTimer =
            0;

        recoveryTimer =
            0;

        rb.linearVelocity =
            Vector2.zero;

        if (
            attackHitbox != null
        )
        {
            attackHitbox.SetActive(
                false
            );
        }

        controller.LockMovement(
            false
        );

        controller.StopMovement();

        controller.PlayAnimation(
            "idle"
        );

        currentAnim = "";
    }


    void PlayAnimation(
        string animName)
    {
        if (animator == null)
            return;

        if (
            currentAnim ==
            animName
        )
            return;

        currentAnim =
            animName;

        animator.Play(
            animName);
    }

    void StartRecovery()
    {
        isPreparingAttack =
            false;

        isAttacking =
            false;

        attackTimer =
            0;

        rb.linearVelocity =
            Vector2.zero;

        if (
            attackHitbox != null
        )
        {
            attackHitbox.SetActive(
                false
            );
        }

        isRecovering =
            true;

        recoveryTimer =
            recoveryTime;

        controller.LockMovement(
            true
        );

        controller.PlayAnimation(
            "idle"
        );

        currentAnim = "";
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);
    }
}