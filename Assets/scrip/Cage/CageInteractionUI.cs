using System.Collections;
using TMPro;
using UnityEngine;

public class CageInteractionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject root;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI hintText;

    [Header("Feedback")]
    public float notEnoughFeedbackDuration = 1.5f;

    private Coroutine _feedbackRoutine;

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        Hide();
    }

    public void Show(CageRequirement requirement)
    {
        if (root != null)
            root.SetActive(true);

        Refresh(requirement);
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

    public void Refresh(CageRequirement requirement)
    {
        if (requirement == null)
            return;

        string label = CagePaymentHelper.GetRequirementLabel(requirement);
        int current = CagePaymentHelper.GetCurrentAmount(requirement);
        bool canAfford = CagePaymentHelper.CanAfford(requirement);

        if (promptText != null)
            promptText.text = $"Giao nộp {requirement.amount} {label} để giải cứu con tin";

        if (progressText != null)
            progressText.text = $"Hiện có: {current}/{requirement.amount}";

        if (hintText != null)
            hintText.text = canAfford
                ? "Nhấn E để giao nộp vật phẩm"
                : "Chưa đủ vật phẩm";
    }

    public void ShowNotEnoughFeedback(CageRequirement requirement)
    {
        if (_feedbackRoutine != null)
            StopCoroutine(_feedbackRoutine);

        _feedbackRoutine = StartCoroutine(NotEnoughFeedbackRoutine(requirement));
    }

    private IEnumerator NotEnoughFeedbackRoutine(CageRequirement requirement)
    {
        if (hintText != null)
            hintText.text = "Không đủ vật phẩm";

        yield return new WaitForSeconds(notEnoughFeedbackDuration);

        if (root != null && root.activeSelf)
            Refresh(requirement);

        _feedbackRoutine = null;
    }
}
