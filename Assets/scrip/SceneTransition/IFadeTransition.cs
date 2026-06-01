using System.Collections;

namespace SceneTransition
{
    /// <summary>
    /// Hiệu ứng fade (Open/Closed — mở rộng thêm wipe, dissolve sau này).
    /// </summary>
    public interface IFadeTransition
    {
        IEnumerator FadeOut();
        IEnumerator FadeIn();
        void SetImmediate(float alpha);
    }
}
