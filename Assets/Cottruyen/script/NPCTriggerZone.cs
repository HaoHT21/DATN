using UnityEngine;
using System.Collections;

public class NPCTriggerZone : MonoBehaviour
{
    [Header("Cấu hình NPC đích (Kéo NPC vào đây)")]
    public GameObject npcObject;

    [Header("Cấu hình Hiệu ứng (Effects)")]
    public GameObject spawnEffectPrefab;
    public GameObject despawnEffectPrefab;
    public float effectDuration = 1.5f;

    private NPCInteraction npcInteraction;
    private bool hasTriggered = false;

    void Start()
    {
        if (npcObject != null)
        {
            npcInteraction = npcObject.GetComponent<NPCInteraction>();
            npcObject.SetActive(false); // Ẩn NPC lúc đầu
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(SpawnNPCRoutine());
        }
    }

    IEnumerator SpawnNPCRoutine()
    {
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, npcObject.transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        yield return new WaitForSecondsRealtime(effectDuration * 0.5f);

        if (npcObject != null) npcObject.SetActive(true);

        yield return new WaitForSecondsRealtime(effectDuration * 0.5f);

        if (npcInteraction != null)
        {
            // Truyền chính TriggerZone này vào NPC để nó biết đường phản hồi
            npcInteraction.RegisterTriggerZone(this);
            npcInteraction.Interact();
        }
    }

    public void OnDialogueComplete()
    {
        StartCoroutine(DespawnNPCRoutine());
    }

    IEnumerator DespawnNPCRoutine()
    {
        if (despawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(despawnEffectPrefab, npcObject.transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        yield return new WaitForSecondsRealtime(effectDuration);

        if (npcObject != null) npcObject.SetActive(false);
        Destroy(gameObject);
    }
}