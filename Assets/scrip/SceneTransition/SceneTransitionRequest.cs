namespace SceneTransition
{
    /// <summary>
    /// Tham số cho một lần chuyển cảnh (immutable data).
    /// </summary>
    public readonly struct SceneTransitionRequest
    {
        public string SceneName { get; }
        public SceneTransitionMode Mode { get; }
        public bool UseFadeOut { get; }
        public bool UseFadeIn { get; }

        public SceneTransitionRequest(
            string sceneName,
            SceneTransitionMode mode = SceneTransitionMode.Asynchronous,
            bool useFadeOut = true,
            bool useFadeIn = true)
        {
            SceneName = sceneName;
            Mode = mode;
            UseFadeOut = useFadeOut;
            UseFadeIn = useFadeIn;
        }
    }
}
