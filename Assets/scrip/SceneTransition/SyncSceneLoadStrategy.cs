using System.Collections;
using UnityEngine.SceneManagement;

namespace SceneTransition
{
    /// <summary>
    /// Load đồng bộ — đơn giản, phù hợp scene nhỏ hoặc build nhẹ.
    /// </summary>
    public sealed class SyncSceneLoadStrategy : ISceneLoadStrategy
    {
        public IEnumerator Load(string sceneName, ISceneLoadProgressReporter reporter)
        {
            reporter?.SetLoadingVisible(false);
            reporter?.ReportProgress(0f);

            SceneManager.LoadScene(sceneName);

            reporter?.ReportProgress(1f);
            yield break;
        }
    }
}
