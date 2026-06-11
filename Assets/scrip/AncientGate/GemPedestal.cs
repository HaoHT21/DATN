using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GemPedestal : MonoBehaviour
{
    [Header("Yêu cầu ngọc")]
    public ItemData requiredGem;

    [Header("Tham chiếu")]
    public PedestalInteractionUI interactionUI;
    public GameObject gemVisual;
    public SpriteRenderer pedestalGlow;
    public Collider2D interactionTrigger;

    [Header("Âm thanh")]
    public AudioClip placeSound;
    public AudioClip notEnoughSound;

    public PedestalState State { get; private set; } = PedestalState.Empty;

    private bool _playerInRange;

    private void Awake()
    {
        if (interactionTrigger == null)
            interactionTrigger = GetComponent<Collider2D>();
    }

    private void Start()
    {
        ApplyStateVisuals();
        GateManager.Instance?.RegisterPedestal(this);
    }

    private void OnDestroy()
    {
        GateManager.Instance?.UnregisterPedestal(this);
    }

    private void Update()
    {
        if (!_playerInRange)
            return;

        if (!GameplayInputGate.CanProcessInput)
            return;

        interactionUI?.Refresh(this);

        if (State == PedestalState.Empty && Input.GetKeyDown(KeyCode.E))
            TryPlaceGem();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerInRange = true;
        interactionUI?.Show(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerInRange = false;
        interactionUI?.Hide();
    }

    private void TryPlaceGem()
    {
        if (State != PedestalState.Empty || requiredGem == null)
            return;

        if (!GemInventoryHelper.HasGem(requiredGem))
        {
            interactionUI?.ShowNotEnoughFeedback(this);
            PlaySound(notEnoughSound);
            return;
        }

        if (!GemInventoryHelper.TryConsumeGem(requiredGem))
        {
            interactionUI?.ShowNotEnoughFeedback(this);
            PlaySound(notEnoughSound);
            return;
        }

        Activate();
        PlaySound(placeSound);
        interactionUI?.ShowPlacedFeedback(this);
        GateManager.Instance?.OnPedestalActivated(this);
    }

    public void Activate()
    {
        if (State == PedestalState.Filled)
            return;

        State = PedestalState.Filled;
        ApplyStateVisuals();
    }

    private void ApplyStateVisuals()
    {
        if (gemVisual != null)
            gemVisual.SetActive(State == PedestalState.Filled);

        if (pedestalGlow != null)
            pedestalGlow.enabled = State == PedestalState.Filled;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }
}
