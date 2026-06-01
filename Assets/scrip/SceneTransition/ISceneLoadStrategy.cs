using System.Collections;

namespace SceneTransition
{
    /// <summary>
    /// Chiến lược load scene (Strategy Pattern).
    /// </summary>
    public interface ISceneLoadStrategy
    {
        IEnumerator Load(string sceneName, ISceneLoadProgressReporter reporter);
    }

    public interface ISceneLoadProgressReporter
    {
        void ReportProgress(float normalizedProgress);
        void SetLoadingVisible(bool visible);
    }
}
