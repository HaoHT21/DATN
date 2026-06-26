using TMPro;
using UnityEngine;

public class ChestInteractionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject root;
    public TextMeshProUGUI promptText;

    [Header("Nội dung")]
    public string closedPrompt = "Nhấn E để mở rương";

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        Hide();
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);

        if (promptText != null)
            promptText.text = closedPrompt;
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}
