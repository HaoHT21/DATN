using System.Collections;
using UnityEngine;

namespace SceneTransition
{
    /// <summary>
    /// Fade bằng CanvasGroup.alpha (0 = trong suốt, 1 = che kín màn hình).
    /// </summary>
    public sealed class CanvasGroupFadeTransition : IFadeTransition
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly float _duration;
        private readonly AnimationCurve _curve;

        public CanvasGroupFadeTransition(CanvasGroup canvasGroup, float duration, AnimationCurve curve)
        {
            _canvasGroup = canvasGroup;
            _duration = Mathf.Max(0.01f, duration);
            _curve = curve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        public IEnumerator FadeOut()
        {
            yield return AnimateAlpha(0f, 1f);
        }

        public IEnumerator FadeIn()
        {
            yield return AnimateAlpha(1f, 0f);
        }

        public void SetImmediate(float alpha)
        {
            if (_canvasGroup == null)
                return;

            _canvasGroup.alpha = Mathf.Clamp01(alpha);
            ApplyRaycastState(alpha);
        }

        private IEnumerator AnimateAlpha(float from, float to)
        {
            if (_canvasGroup == null)
                yield break;

            float elapsed = 0f;
            _canvasGroup.alpha = from;
            ApplyRaycastState(from);

            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float eased = _curve.Evaluate(t);
                float alpha = Mathf.Lerp(from, to, eased);
                _canvasGroup.alpha = alpha;
                ApplyRaycastState(alpha);
                yield return null;
            }

            _canvasGroup.alpha = to;
            ApplyRaycastState(to);
        }

        private void ApplyRaycastState(float alpha)
        {
            bool blocking = alpha > 0.01f;
            _canvasGroup.blocksRaycasts = blocking;
            _canvasGroup.interactable = blocking;
        }
    }
}
