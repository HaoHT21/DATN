using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneTransition
{
    /// <summary>
    /// Điều phối chuyển cảnh toàn dự án: Fade Out → Load → Fade In.
    /// Singleton + DontDestroyOnLoad.
    /// </summary>
    public sealed class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Fade")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.45f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool fadeInOnFirstSceneStart = true;

        [Header("Async Loading")]
        [SerializeField] private float loadingScreenDelay = 0.5f;

        [Header("References")]
        [SerializeField] private SceneTransitionUI transitionUI;

        public bool IsTransitioning { get; private set; }

        public event Action<string> OnTransitionStarted;
        public event Action<string> OnTransitionCompleted;

        private IFadeTransition _fadeTransition;
        private ITransitionInputBlocker _inputBlocker;
        private SyncSceneLoadStrategy _syncLoader;
        private AsyncSceneLoadStrategy _asyncLoader;
        private Coroutine _activeTransition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDependencies();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            if (!fadeInOnFirstSceneStart || fadeCanvasGroup == null)
                return;

            _fadeTransition.SetImmediate(1f);
            StartCoroutine(FadeInOnlyRoutine());
        }

        private void InitializeDependencies()
        {
            if (fadeCanvasGroup == null)
            {
                Debug.LogError("[SceneTransitionManager] Thiếu CanvasGroup fade. Gán Fade Overlay trong Inspector.");
                return;
            }

            _fadeTransition = new CanvasGroupFadeTransition(fadeCanvasGroup, fadeDuration, fadeCurve);
            _inputBlocker = new TransitionInputBlocker(fadeCanvasGroup);
            _syncLoader = new SyncSceneLoadStrategy();
            _asyncLoader = new AsyncSceneLoadStrategy(loadingScreenDelay);

            _fadeTransition.SetImmediate(0f);
        }

        /// <summary>Chuyển scene bất đồng bộ (khuyến nghị).</summary>
        public void LoadScene(string sceneName)
        {
            LoadScene(new SceneTransitionRequest(sceneName, SceneTransitionMode.Asynchronous));
        }

        /// <summary>Chuyển scene đồng bộ.</summary>
        public void LoadSceneSync(string sceneName)
        {
            LoadScene(new SceneTransitionRequest(sceneName, SceneTransitionMode.Synchronous));
        }

        public void LoadScene(SceneTransitionRequest request)
        {
            if (IsTransitioning)
            {
                Debug.LogWarning($"[SceneTransition] Đang chuyển cảnh, bỏ qua yêu cầu tới '{request.SceneName}'.");
                return;
            }

            if (string.IsNullOrWhiteSpace(request.SceneName))
            {
                Debug.LogError("[SceneTransition] Tên scene rỗng.");
                return;
            }

            if (_activeTransition != null)
                StopCoroutine(_activeTransition);

            _activeTransition = StartCoroutine(TransitionRoutine(request));
        }

        private IEnumerator TransitionRoutine(SceneTransitionRequest request)
        {
            IsTransitioning = true;
            OnTransitionStarted?.Invoke(request.SceneName);

            RestoreGameplayTime();
            _inputBlocker?.SetBlocked(true);

            if (request.UseFadeOut && _fadeTransition != null)
                yield return _fadeTransition.FadeOut();
            else
                _fadeTransition?.SetImmediate(1f);

            ISceneLoadStrategy loader = request.Mode == SceneTransitionMode.Synchronous
                ? _syncLoader
                : (ISceneLoadStrategy)_asyncLoader;

            ISceneLoadProgressReporter reporter = transitionUI != null ? transitionUI : null;
            yield return loader.Load(request.SceneName, reporter);

            RestoreGameplayTime();

            if (request.UseFadeIn && _fadeTransition != null)
                yield return _fadeTransition.FadeIn();
            else
                _fadeTransition?.SetImmediate(0f);

            _inputBlocker?.SetBlocked(false);
            IsTransitioning = false;
            _activeTransition = null;

            OnTransitionCompleted?.Invoke(request.SceneName);
        }

        private IEnumerator FadeInOnlyRoutine()
        {
            IsTransitioning = true;
            _inputBlocker?.SetBlocked(true);
            yield return _fadeTransition.FadeIn();
            _inputBlocker?.SetBlocked(false);
            IsTransitioning = false;
        }

        private static void RestoreGameplayTime()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
