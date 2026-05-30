using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SceneTransition
{
    /// <summary>
    /// Loading screen và progress bar (Single Responsibility — chỉ hiển thị).
    /// </summary>
    public sealed class SceneTransitionUI : MonoBehaviour, ISceneLoadProgressReporter
    {
        [Header("Loading")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image progressFillImage;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Text legacyProgressLabel;
        [SerializeField] private bool showPercentageText = true;

        private void Awake()
        {
            SetLoadingVisible(false);
            ReportProgress(0f);
        }

        public void ReportProgress(float normalizedProgress)
        {
            float value = Mathf.Clamp01(normalizedProgress);

            if (progressBar != null)
                progressBar.value = value;

            if (progressFillImage != null)
                progressFillImage.fillAmount = value;

            if (!showPercentageText)
                return;

            string text = $"{Mathf.RoundToInt(value * 100f)}%";
            if (progressLabel != null)
                progressLabel.text = text;
            if (legacyProgressLabel != null)
                legacyProgressLabel.text = text;
        }

        public void SetLoadingVisible(bool visible)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(visible);
        }
    }
}
