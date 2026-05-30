namespace SceneTransition
{
    /// <summary>
    /// Cách load scene sau khi fade out hoàn tất.
    /// </summary>
    public enum SceneTransitionMode
    {
        /// <summary>LoadScene — chặn main thread đến khi xong.</summary>
        Synchronous,

        /// <summary>LoadSceneAsync — có progress bar và loading screen.</summary>
        Asynchronous
    }
}
