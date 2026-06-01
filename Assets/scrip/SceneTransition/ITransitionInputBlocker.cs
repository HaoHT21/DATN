namespace SceneTransition
{
    /// <summary>
    /// Chặn tương tác người chơi trong lúc chuyển cảnh.
    /// </summary>
    public interface ITransitionInputBlocker
    {
        void SetBlocked(bool blocked);
    }
}
