using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneTransition
{
    /// <summary>
    /// Load bất đồng bộ với progress chuẩn hóa (Unity báo 0–0.9 khi load, 0.9–1 khi activate).
    /// </summary>
    public sealed class AsyncSceneLoadStrategy : ISceneLoadStrategy
    {
        private readonly float _loadingScreenDelay;

        public AsyncSceneLoadStrategy(float loadingScreenDelay)
        {
            _loadingScreenDelay = Mathf.Max(0f, loadingScreenDelay);
        }

        public IEnumerator Load(string sceneName, ISceneLoadProgressReporter reporter)
        {
            reporter?.SetLoadingVisible(false);
            reporter?.ReportProgress(0f);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                Debug.LogError($"[SceneTransition] Không load được scene: {sceneName}. Kiểm tra Build Settings.");
                yield break;
            }

            operation.allowSceneActivation = false;
            float loadStartTime = Time.unscaledTime;
            bool loadingUiShown = false;

            while (!operation.isDone)
            {
                float normalized = GetNormalizedProgress(operation);

                if (!loadingUiShown && Time.unscaledTime - loadStartTime >= _loadingScreenDelay)
                {
                    loadingUiShown = true;
                    reporter?.SetLoadingVisible(true);
                }

                reporter?.ReportProgress(normalized);

                if (operation.progress >= 0.9f)
                    operation.allowSceneActivation = true;

                yield return null;
            }

            reporter?.ReportProgress(1f);
            reporter?.SetLoadingVisible(false);
        }

        public static float GetNormalizedProgress(AsyncOperation operation)
        {
            if (operation == null)
                return 0f;

            return operation.progress >= 0.9f ? 1f : operation.progress / 0.9f;
        }
    }
}
