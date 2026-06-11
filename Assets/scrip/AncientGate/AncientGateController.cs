using System.Collections;
using SceneTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class AncientGateController : MonoBehaviour
{
    [Header("Chuyển Scene")]
    public string nextSceneName;
    [Tooltip("Dùng SceneTransitionManager nếu có trong scene.")]
    public bool useSceneTransitionManager = true;

    [Header("Collider")]
    public Collider2D blockingCollider;
    public Collider2D portalTrigger;

    [Header("Animation")]
    public Animator gateAnimator;
    public string openTriggerName = "Open";
    public float openAnimationFallbackDuration = 2f;

    [Header("Âm thanh")]
    public AudioClip unlockSound;

    public GateState State { get; private set; } = GateState.Closed;

    private Coroutine _openingRoutine;

    private void Awake()
    {
        if (gateAnimator == null)
            gateAnimator = GetComponentInChildren<Animator>();

        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider2D>();

        ApplyStateColliders();
    }

    public void OpenGate()
    {
        if (State != GateState.Closed)
            return;

        State = GateState.Opening;
        PlaySound(unlockSound);

        if (gateAnimator != null && !string.IsNullOrEmpty(openTriggerName))
            gateAnimator.SetTrigger(openTriggerName);

        if (_openingRoutine != null)
            StopCoroutine(_openingRoutine);

        _openingRoutine = StartCoroutine(OpeningFallbackRoutine());
    }

    public void OnOpenAnimationComplete()
    {
        if (State != GateState.Opening)
            return;

        if (_openingRoutine != null)
        {
            StopCoroutine(_openingRoutine);
            _openingRoutine = null;
        }

        CompleteOpening();
    }

    private IEnumerator OpeningFallbackRoutine()
    {
        yield return new WaitForSeconds(openAnimationFallbackDuration);
        OnOpenAnimationComplete();
    }

    private void CompleteOpening()
    {
        State = GateState.Open;
        ApplyStateColliders();
    }

    private void ApplyStateColliders()
    {
        if (blockingCollider != null)
            blockingCollider.enabled = State == GateState.Closed;

        if (portalTrigger != null)
            portalTrigger.enabled = State == GateState.Open;
    }

    public void OnPlayerEnterPortal()
    {
        if (State != GateState.Open)
            return;

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("[AncientGate] Chưa cấu hình nextSceneName.");
            return;
        }

        if (useSceneTransitionManager && SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(nextSceneName);
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }
}
