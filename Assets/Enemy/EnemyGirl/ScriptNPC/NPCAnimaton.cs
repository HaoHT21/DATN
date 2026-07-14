using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCAnimation : MonoBehaviour
{
    private Animator animator;

    private string currentAnimation = "";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayIdle()
    {
        Play("idle");
    }

    public void PlayRun()
    {
        Play("run");
    }

    private void Play(string animationName)
    {
        if (currentAnimation == animationName)
            return;

        currentAnimation = animationName;
        animator.Play(animationName);
    }
}