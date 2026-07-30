using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AncientGateController : MonoBehaviour
{
    [Header("Portals")]
    public GameObject portalBlack;
    public GameObject portalWhite;

    [Header("Animation")]
    public Animator gateAnimator;

    [Header("Collider")]
    public Collider2D blockingCollider;

    [Header("Âm thanh")]
    public AudioClip unlockSound;

    public GateState State { get; private set; } = GateState.Closed;

    private GameObject _activePortal;

    private void Awake()
    {
        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider2D>();

        HideAllPortals();
        ApplyStateColliders();
    }

    public void OpenGateWithBlackPortal()
    {
        if (State != GateState.Closed) return;

        State = GateState.Open;
        _activePortal = portalBlack;

        PlaySound(unlockSound);
        ApplyStateColliders();

        // Chỉ trigger animation, CHƯA bật portal vội
        if (gateAnimator != null)
            gateAnimator.Play("Open");
    }

    public void OpenGateWithWhitePortal()
    {
        if (State != GateState.Closed) return;

        State = GateState.Open;
        _activePortal = portalWhite;

        PlaySound(unlockSound);
        ApplyStateColliders();

        // Chỉ trigger animation, CHƯA bật portal vội
        if (gateAnimator != null)
            gateAnimator.Play("Open");
    }

    /// <summary>
    /// Hàm này sẽ được gọi từ GateAnimationEvents khi animation chạy tới event
    /// </summary>
    public void ShowActivePortal()
    {
        HideAllPortals();

        if (_activePortal != null)
        {
            _activePortal.SetActive(true);
            Debug.Log($"[AncientGate] Đã bật Portal: {_activePortal.name}");
        }
    }

    private void HideAllPortals()
    {
        if (portalWhite != null) portalWhite.SetActive(false);
        if (portalBlack != null) portalBlack.SetActive(false);
    }

    private void ApplyStateColliders()
    {
        if (blockingCollider != null)
            blockingCollider.enabled = State == GateState.Closed;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySound(clip);
    }
}