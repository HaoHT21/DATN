using System.Collections;
using UnityEngine;

public class TrapLock : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    public float lockTime = 2f;

    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated)
            return;

        if (!other.CompareTag("Player"))
            return;

        activated = true;

        if (animator != null)
            animator.Play("Lock");

        PlayerController player =
            other.GetComponent<PlayerController>();

        if (player != null)
            StartCoroutine(LockPlayer(player));
    }

    IEnumerator LockPlayer(PlayerController player)
    {
        player.enabled = false;

        yield return new WaitForSeconds(lockTime);

        player.enabled = true;
    }
}