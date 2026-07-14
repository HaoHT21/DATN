using UnityEngine;
using System.Collections;

public class BossDecisionTrigger1 : MonoBehaviour
{
    public GameObject deathEffectPrefab;
    public GameObject executedSpawnPrefab;
    public Transform spawnPoint;
    public NPCTriggerZone nextDialogueTriggerZone;
    public string deathAnimationStateName = "Death Animation";

    private BossHeath bossHealth;
    private Animator animator;

    private bool isTriggered = false;

    void Awake()
    {
        bossHealth = GetComponent<BossHeath>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isTriggered) return;
        if (bossHealth == null) return;

        if (bossHealth.currentHeath <= 0)
        {
            StartCoroutine(HandleSequence());
        }
    }

    IEnumerator HandleSequence()
    {
        isTriggered = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        if (animator != null)
            animator.Play(deathAnimationStateName);

        // Chờ animation chết
        yield return new WaitForSeconds(1.5f);

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;

        yield return new WaitForSeconds(0.5f);

        GameObject spawnedNPC = null;

        if (executedSpawnPrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            spawnedNPC = Instantiate(executedSpawnPrefab, pos, rot);
            spawnedNPC.SetActive(false);
        }

        if (nextDialogueTriggerZone != null)
        {
            nextDialogueTriggerZone.gameObject.SetActive(true);

            if (spawnedNPC != null)
                nextDialogueTriggerZone.TriggerDialogueFromBoss(spawnedNPC);
        }
        else
        {
            if (spawnedNPC != null)
                spawnedNPC.SetActive(true);
        }

        Destroy(gameObject);
    }
}