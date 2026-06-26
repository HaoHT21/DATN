using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AncientGateController : MonoBehaviour
{
    [Header("Cổng")]
    [Tooltip("GameObject cổng hiển thị sau khi mở (nên để inactive lúc đầu).")]
    public GameObject gateObject;

    [Header("Collider")]
    public Collider2D blockingCollider;

    [Header("Âm thanh")]
    public AudioClip unlockSound;

    public GateState State { get; private set; } = GateState.Closed;

    private void Awake()
    {
        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider2D>();

        if (gateObject != null)
            gateObject.SetActive(false);

        ApplyStateColliders();
    }

    public void OpenGate()
    {
        if (State != GateState.Closed)
            return;

        PlaySound(unlockSound);
        CompleteOpening();
    }

    private void CompleteOpening()
    {
        State = GateState.Open;

        if (gateObject != null)
            gateObject.SetActive(true);

        ApplyStateColliders();
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
