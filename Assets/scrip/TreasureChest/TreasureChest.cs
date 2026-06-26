using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TreasureChest : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Animator chestAnimator;
    public Collider2D interactionTrigger;
    public Transform lootSpawnPoint;
    public ChestInteractionUI interactionUI;

    [Header("Vật phẩm")]
    public GameObject lootPrefab;
    public bool applyLootPop = true;

    [Header("Animation")]
    public string openTriggerName = "Open";
    public float openAnimationFallbackDuration = 1.5f;

    [Header("Âm thanh")]
    public AudioClip openSound;

    public ChestState State { get; private set; } = ChestState.Closed;

    private bool _playerInRange;
    private bool _lootSpawned;
    private Coroutine _openingRoutine;

    private void Awake()
    {
        if (interactionTrigger == null)
            interactionTrigger = GetComponent<Collider2D>();

        if (chestAnimator == null)
            chestAnimator = GetComponentInChildren<Animator>();

        if (lootSpawnPoint == null)
            lootSpawnPoint = transform;
    }

    private void Start()
    {
        ApplyStateVisuals();
    }

    private void Update()
    {
        if (!_playerInRange || State != ChestState.Closed)
            return;

        if (!GameplayInputGate.CanProcessInput)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            TryOpen();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || State != ChestState.Closed)
            return;

        _playerInRange = true;
        interactionUI?.Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerInRange = false;
        interactionUI?.Hide();
    }

    private void TryOpen()
    {
        if (State != ChestState.Closed)
            return;

        BeginOpening();
    }

    private void BeginOpening()
    {
        State = ChestState.Opening;
        _playerInRange = false;
        interactionUI?.Hide();

        if (interactionTrigger != null)
            interactionTrigger.enabled = false;

        PlaySound(openSound);

        if (chestAnimator != null && !string.IsNullOrEmpty(openTriggerName))
            chestAnimator.SetTrigger(openTriggerName);

        if (_openingRoutine != null)
            StopCoroutine(_openingRoutine);

        _openingRoutine = StartCoroutine(OpeningFallbackRoutine());
    }

    public void OnOpenAnimationComplete()
    {
        if (State != ChestState.Opening)
            return;

        if (_openingRoutine != null)
        {
            StopCoroutine(_openingRoutine);
            _openingRoutine = null;
        }

        SpawnLoot();
        State = ChestState.Opened;
        ApplyStateVisuals();
    }

    private IEnumerator OpeningFallbackRoutine()
    {
        yield return new WaitForSeconds(openAnimationFallbackDuration);
        OnOpenAnimationComplete();
    }

    private void SpawnLoot()
    {
        if (_lootSpawned || lootPrefab == null)
            return;

        _lootSpawned = true;

        Vector3 spawnPos = lootSpawnPoint != null ? lootSpawnPoint.position : transform.position;
        GameObject loot = Instantiate(lootPrefab, spawnPos, Quaternion.identity);

        if (applyLootPop && loot.GetComponent<ChestLootPop>() == null)
            loot.AddComponent<ChestLootPop>();
    }

    private void ApplyStateVisuals()
    {
        switch (State)
        {
            case ChestState.Closed:
                if (interactionTrigger != null)
                    interactionTrigger.enabled = true;
                break;

            case ChestState.Opening:
            case ChestState.Opened:
                if (interactionTrigger != null)
                    interactionTrigger.enabled = false;
                break;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Transform point = lootSpawnPoint != null ? lootSpawnPoint : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(point.position, 0.15f);
    }
}
