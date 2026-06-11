using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CageController : MonoBehaviour
{
    [Header("Yêu cầu giải cứu")]
    public CageRequirement requirement = new CageRequirement();

    [Header("Tham chiếu")]
    public HostageController hostage;
    public CageInteractionUI interactionUI;
    public Animator cageAnimator;
    public Collider2D interactionTrigger;

    [Header("Animation")]
    public string openTriggerName = "Open";
    public float openAnimationFallbackDuration = 1.5f;

    [Header("Âm thanh")]
    public AudioClip openSound;
    public AudioClip notEnoughSound;

    public CageState State { get; private set; } = CageState.Locked;

    private bool _playerInRange;
    private Coroutine _openingRoutine;

    private void Awake()
    {
        if (interactionTrigger == null)
            interactionTrigger = GetComponent<Collider2D>();

        if (cageAnimator == null)
            cageAnimator = GetComponentInChildren<Animator>();

        if (hostage == null)
            hostage = GetComponentInChildren<HostageController>();
    }

    private void Start()
    {
        ApplyStateVisuals();
    }

    private void Update()
    {
        if (!_playerInRange || State != CageState.Locked)
            return;

        if (!GameplayInputGate.CanProcessInput)
            return;

        interactionUI?.Refresh(requirement);

        if (Input.GetKeyDown(KeyCode.E))
            TrySubmitPayment();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || State != CageState.Locked)
            return;

        _playerInRange = true;
        interactionUI?.Show(requirement);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerInRange = false;
        interactionUI?.Hide();
    }

    private void TrySubmitPayment()
    {
        if (!CagePaymentHelper.CanAfford(requirement))
        {
            interactionUI?.ShowNotEnoughFeedback(requirement);
            PlaySound(notEnoughSound);
            return;
        }

        if (!CagePaymentHelper.TryPay(requirement))
        {
            interactionUI?.ShowNotEnoughFeedback(requirement);
            PlaySound(notEnoughSound);
            return;
        }

        BeginOpening();
    }

    private void BeginOpening()
    {
        State = CageState.Opening;
        _playerInRange = false;
        interactionUI?.Hide();

        if (interactionTrigger != null)
            interactionTrigger.enabled = false;

        PlaySound(openSound);

        if (cageAnimator != null && !string.IsNullOrEmpty(openTriggerName))
            cageAnimator.SetTrigger(openTriggerName);

        if (_openingRoutine != null)
            StopCoroutine(_openingRoutine);

        _openingRoutine = StartCoroutine(OpeningFallbackRoutine());
    }

    public void OnOpenAnimationComplete()
    {
        if (State != CageState.Opening)
            return;

        if (_openingRoutine != null)
        {
            StopCoroutine(_openingRoutine);
            _openingRoutine = null;
        }

        CompleteRescue();
    }

    private IEnumerator OpeningFallbackRoutine()
    {
        yield return new WaitForSeconds(openAnimationFallbackDuration);
        OnOpenAnimationComplete();
    }

    private void CompleteRescue()
    {
        State = CageState.Rescued;

        if (hostage != null)
        {
            hostage.transform.SetParent(null);
            hostage.OnRescued();
        }

        ApplyStateVisuals();
    }

    private void ApplyStateVisuals()
    {
        switch (State)
        {
            case CageState.Locked:
                if (interactionTrigger != null)
                    interactionTrigger.enabled = true;
                break;

            case CageState.Opening:
            case CageState.Rescued:
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
}
