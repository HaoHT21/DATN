using UnityEngine;

public class BossIdleState : IBossEnemyState
{
    public BossEnemyState StateId => BossEnemyState.Idle;

    public void Enter(BossEnemyContext context)
    {
        context.StopMovement();
        context.PlayAnimation(context.AI.idleAnimation);
    }

    public void Update(BossEnemyContext context)
    {
        context.StopMovement();

        if (context.IsPlayerInDetectionRange())
            context.AI.StateMachine.ChangeState(BossEnemyState.Chase);
    }

    public void Exit(BossEnemyContext context) { }
}

public class BossChaseState : IBossEnemyState
{
    public BossEnemyState StateId => BossEnemyState.Chase;

    public void Enter(BossEnemyContext context)
    {
        context.PlayAnimation(context.AI.chaseAnimation);
    }

    public void Update(BossEnemyContext context)
    {
        if (!context.IsPlayerInDetectionRange())
        {
            context.AI.StateMachine.ChangeState(BossEnemyState.Idle);
            return;
        }

        context.FacePlayer();

        if (context.IsPlayerInAttackRange())
        {
            context.StopMovement();
            context.AI.StateMachine.ChangeState(context.PickRandomAttack());
            return;
        }

        Vector2 direction = (context.PlayerTarget.position - context.Transform.position).normalized;
        context.Rigidbody.linearVelocity = direction * context.AI.moveSpeed;
        context.PlayAnimation(context.AI.chaseAnimation);
    }

    public void Exit(BossEnemyContext context)
    {
        context.StopMovement();
    }
}

public class BossAttack1State : IBossEnemyState
{
    public BossEnemyState StateId => BossEnemyState.Attack1;

    public void Enter(BossEnemyContext context)
    {
        context.AttackAnimationFinished = false;
        context.StopMovement();
        context.FacePlayer();
        context.PlayAnimation(context.AI.attack1Animation);
        SetHitboxActive(context.AI.attack1Hitbox, false);
    }

    public void Update(BossEnemyContext context)
    {
        context.StopMovement();
        context.FacePlayer();

        if (context.AttackAnimationFinished || context.IsAnimationFinished(context.AI.attack1Animation))
            context.AI.StateMachine.ChangeState(BossEnemyState.AttackRecovery);
    }

    public void Exit(BossEnemyContext context)
    {
        context.StopMovement();
        SetHitboxActive(context.AI.attack1Hitbox, false);
    }

    private static void SetHitboxActive(GameObject hitbox, bool active)
    {
        if (hitbox != null)
            hitbox.SetActive(active);
    }
}

public class BossAttack2State : IBossEnemyState
{
    public BossEnemyState StateId => BossEnemyState.Attack2;

    public void Enter(BossEnemyContext context)
    {
        context.AttackAnimationFinished = false;
        context.StopMovement();
        context.FacePlayer();
        context.PlayAnimation(context.AI.attack2Animation);
        SetHitboxActive(context.AI.attack2Hitbox, false);
    }

    public void Update(BossEnemyContext context)
    {
        context.StopMovement();
        context.FacePlayer();

        if (context.AttackAnimationFinished || context.IsAnimationFinished(context.AI.attack2Animation))
            context.AI.StateMachine.ChangeState(BossEnemyState.AttackRecovery);
    }

    public void Exit(BossEnemyContext context)
    {
        context.StopMovement();
        SetHitboxActive(context.AI.attack2Hitbox, false);
    }

    private static void SetHitboxActive(GameObject hitbox, bool active)
    {
        if (hitbox != null)
            hitbox.SetActive(active);
    }
}

public class BossAttackRecoveryState : IBossEnemyState
{
    private float _timer;

    public BossEnemyState StateId => BossEnemyState.AttackRecovery;

    public void Enter(BossEnemyContext context)
    {
        _timer = context.AI.attackRecoveryTime;
        context.StopMovement();
        context.PlayAnimation(context.AI.idleAnimation);
    }

    public void Update(BossEnemyContext context)
    {
        context.StopMovement();

        if (context.AI.facePlayerDuringRecovery)
            context.FacePlayer();

        _timer -= Time.deltaTime;
        if (_timer > 0f)
            return;

        if (!context.IsPlayerInDetectionRange())
            context.AI.StateMachine.ChangeState(BossEnemyState.Idle);
        else if (context.IsPlayerInAttackRange())
            context.AI.StateMachine.ChangeState(context.PickRandomAttack());
        else
            context.AI.StateMachine.ChangeState(BossEnemyState.Chase);
    }

    public void Exit(BossEnemyContext context) { }
}

public class BossHurtState : IBossEnemyState
{
    public BossEnemyState StateId => BossEnemyState.Hurt;

    public void Enter(BossEnemyContext context)
    {
        context.HurtAnimationFinished = false;
        context.StopMovement();
        context.PlayAnimation(context.AI.hurtAnimation);
    }

    public void Update(BossEnemyContext context)
    {
        context.StopMovement();

        if (!context.HurtAnimationFinished && !context.IsAnimationFinished(context.AI.hurtAnimation))
            return;

        if (!context.IsPlayerInDetectionRange())
            context.AI.StateMachine.ChangeState(BossEnemyState.Idle);
        else if (context.IsPlayerInAttackRange())
            context.AI.StateMachine.ChangeState(context.PickRandomAttack());
        else
            context.AI.StateMachine.ChangeState(BossEnemyState.Chase);
    }

    public void Exit(BossEnemyContext context) { }
}

public class BossDeathState : IBossEnemyState
{
    public BossEnemyState StateId => BossEnemyState.Death;

    public void Enter(BossEnemyContext context)
    {
        context.StopMovement();
        context.Rigidbody.simulated = false;

        if (context.AI.TryGetComponent(out Collider2D col))
            col.enabled = false;

        context.PlayAnimation(context.AI.deathAnimation);
        context.AI.HandleDeathRewards();
    }

    public void Update(BossEnemyContext context) { }

    public void Exit(BossEnemyContext context) { }
}
