using UnityEngine;

public class BossEnemyContext
{
    public BossEnemyAI AI { get; }

    public Transform Transform => AI.transform;
    public Rigidbody2D Rigidbody => AI.Rigidbody;
    public Animator Animator => AI.Animator;
    public SpriteRenderer Sprite => AI.Sprite;
    public Transform BossVisual => AI.bossVisual;

    public Transform PlayerTarget { get; set; }

    public bool AttackAnimationFinished { get; set; }
    public bool HurtAnimationFinished { get; set; }

    public BossEnemyContext(BossEnemyAI ai)
    {
        AI = ai;
    }

    public void StopMovement()
    {
        Rigidbody.linearVelocity = Vector2.zero;
    }

    public float DistanceToPlayer()
    {
        if (PlayerTarget == null) return float.MaxValue;
        return Vector2.Distance(Transform.position, PlayerTarget.position);
    }

    public bool IsPlayerInDetectionRange()
    {
        return PlayerTarget != null && DistanceToPlayer() <= AI.detectionRange;
    }

    public bool IsPlayerInAttackRange()
    {
        return PlayerTarget != null && DistanceToPlayer() <= AI.attackRange;
    }

    public void FacePlayer()
    {
        if (PlayerTarget == null) return;

        Vector2 toPlayer = PlayerTarget.position - Transform.position;
        bool facingLeft = toPlayer.x < -0.1f;
        bool facingRight = toPlayer.x > 0.1f;

        if (BossVisual != null)
        {
            Vector3 rot = BossVisual.localEulerAngles;
            if (facingRight)
                rot.y = 0f;
            else if (facingLeft)
                rot.y = 180f;
            BossVisual.localEulerAngles = rot;
        }
        else if (Sprite != null && (facingLeft || facingRight))
        {
            Sprite.flipX = facingLeft;
        }

        if (facingLeft || facingRight)
            AI.UpdateHitboxFacing(facingLeft);
    }

    public BossEnemyState PickRandomAttack()
    {
        float roll = Random.value;
        return roll <= AI.attack1Weight ? BossEnemyState.Attack1 : BossEnemyState.Attack2;
    }

    public void PlayAnimation(string stateName)
    {
        if (Animator != null && !string.IsNullOrEmpty(stateName))
            Animator.Play(stateName);
    }

    public bool IsAnimationFinished(string stateName, int layer = 0)
    {
        if (Animator == null || string.IsNullOrEmpty(stateName))
            return true;

        AnimatorStateInfo info = Animator.GetCurrentAnimatorStateInfo(layer);
        return info.IsName(stateName) && info.normalizedTime >= 1f && !Animator.IsInTransition(layer);
    }
}
