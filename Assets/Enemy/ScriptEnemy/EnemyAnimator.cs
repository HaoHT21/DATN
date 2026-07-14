//using UnityEngine;

//[RequireComponent(typeof(Animator))]
//public class EnemyAnimator : MonoBehaviour
//{
//    protected EnemyController enemy;
//    protected Animator animator;

//    protected string currentAnimation;

//    protected virtual void Awake()
//    {
//        enemy = GetComponentInParent<EnemyController>();

//        if (enemy == null)
//            enemy = GetComponent<EnemyController>();

//        animator = GetComponent<Animator>();

//        Debug.Log(enemy);
//        Debug.Log(animator);
//    }

//    protected virtual void Update()
//    {
//        if (enemy == null)
//            return;

//        switch (enemy.CurrentState)
//        {
//            case EnemyState.Idle:
//                PlayIdle();
//                break;

//            case EnemyState.Chase:
//            case EnemyState.Circle:
//                PlayRun();
//                break;

//            case EnemyState.Attack:
//                PlayAttack();
//                break;

//            case EnemyState.Hurt:
//                PlayHurt();
//                break;

//            case EnemyState.Death:
//                PlayDeath();
//                break;
//        }
//    }

//    protected virtual void Play(string animationName)
//    {
//        if (currentAnimation == animationName)
//            return;

//        currentAnimation = animationName;

//        animator.Play(animationName);
//    }

//    public virtual void PlayIdle()
//    {
//        Play("idle");
//    }

//    public virtual void PlayRun()
//    {
//        Play("run");
//    }

//    public virtual void PlayAttack()
//    {
//        Play("attack");
//    }

//    public virtual void PlayHurt()
//    {
//        Play("hurt");
//    }

//    public virtual void PlayDeath()
//    {
//        Play("death");
//    }
//}