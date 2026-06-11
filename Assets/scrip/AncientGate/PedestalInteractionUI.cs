using System.Collections;
using TMPro;
using UnityEngine;

public class PedestalInteractionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject root;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI hintText;

    [Header("Feedback")]
    public float placeFeedbackDuration = 2f;
    public float notEnoughFeedbackDuration = 1.5f;

    private Coroutine _feedbackRoutine;

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        Hide();
    }

    public void Show(GemPedestal pedestal)
    {
        if (root != null)
            root.SetActive(true);

        Refresh(pedestal);
    }

    public void Hide()
    {
        if (_feedbackRoutine != null)
        {
            StopCoroutine(_feedbackRoutine);
            _feedbackRoutine = null;
        }

        if (root != null)
            root.SetActive(false);
    }

    public void Refresh(GemPedestal pedestal)
    {
        if (pedestal == null)
            return;

        string label = GemInventoryHelper.GetGemLabel(pedestal.requiredGem);

        if (pedestal.State == PedestalState.Filled)
        {
            if (promptText != null)
                promptText.text = $"{label} đã được đặt";

            if (hintText != null)
                hintText.text = string.Empty;
            return;
        }

        bool hasGem = GemInventoryHelper.HasGem(pedestal.requiredGem);

        if (promptText != null)
            promptText.text = hasGem
                ? $"Nhấn E để đặt {label}"
                : $"Bạn chưa có {label}";

        if (hintText != null)
            hintText.text = hasGem
                ? "Nhấn E"
                : string.Empty;
    }

    public void ShowNotEnoughFeedback(GemPedestal pedestal)
    {
        if (_feedbackRoutine != null)
            StopCoroutine(_feedbackRoutine);

        _feedbackRoutine = StartCoroutine(NotEnoughFeedbackRoutine(pedestal));
    }

    public void ShowPlacedFeedback(GemPedestal pedestal)
    {
        if (_feedbackRoutine != null)
            StopCoroutine(_feedbackRoutine);

        _feedbackRoutine = StartCoroutine(PlacedFeedbackRoutine(pedestal));
    }

    private IEnumerator NotEnoughFeedbackRoutine(GemPedestal pedestal)
    {
        string label = GemInventoryHelper.GetGemLabel(pedestal?.requiredGem);

        if (hintText != null)
            hintText.text = $"Bạn chưa có {label}";

        yield return new WaitForSeconds(notEnoughFeedbackDuration);

        if (root != null && root.activeSelf)
            Refresh(pedestal);

        _feedbackRoutine = null;
    }

    private IEnumerator PlacedFeedbackRoutine(GemPedestal pedestal)
    {
        string label = GemInventoryHelper.GetGemLabel(pedestal?.requiredGem);

        if (promptText != null)
            promptText.text = $"{label} đã được đặt vào phiến đá";

        if (hintText != null)
            hintText.text = string.Empty;

        yield return new WaitForSeconds(placeFeedbackDuration);

        Hide();
        _feedbackRoutine = null;
    }
}
