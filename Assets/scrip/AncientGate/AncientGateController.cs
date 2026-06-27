using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AncientGateController : MonoBehaviour
{
    [Header("Portal")]
    public GameObject portalWhite;

    [Header("Animation")]
    public Animator gateAnimator;

    [Header("Collider")]
    public Collider2D blockingCollider;

    [Header("Âm thanh")]
    public AudioClip unlockSound;

    public GateState State { get; private set; } = GateState.Closed;

    private void Awake()
    {
        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider2D>();

        if (portalWhite != null)
            portalWhite.SetActive(false);

        ApplyStateColliders();
    }

    public void OpenGate()
    {
        if (State != GateState.Closed)
            return;

        State = GateState.Open;

        PlaySound(unlockSound);

        ApplyStateColliders();

        // Chạy animation Open
        if (gateAnimator != null)
            gateAnimator.Play("Open");
    }

    public void ShowPortal()
    {
        if (portalWhite != null)
            portalWhite.SetActive(true);
    }

    private void ApplyStateColliders()
    {
        if (blockingCollider != null)
            blockingCollider.enabled = State == GateState.Closed;
    }

    public void OnPlayerEnterPortal()
    {
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }
}