using UnityEngine;
using UnityEngine.UI;

public class ContinueButton : MonoBehaviour
{
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        if (continueButton == null)
        {
            continueButton = GetComponent<Button>();
        }
    }

    private void Start()
    {
        // Tự động gán sự kiện Click để tránh gán nhầm trong Inspector
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueGame);
        }

        UpdateButtonState();
    }

    private void OnEnable()
    {
        UpdateButtonState();
    }

    public void UpdateButtonState()
    {
        if (continueButton == null) return;

        bool hasSaveData = SaveManager.Instance != null && SaveManager.Instance.HasSave();
        continueButton.interactable = hasSaveData;
    }

    public void ContinueGame()
    {
        if (SaveManager.Instance != null)
        {
            if (SaveManager.Instance.HasSave())
            {
                Debug.Log("<color=green>[CONTINUE]</color> Đang gọi SaveManager.Instance.LoadGame()...");
                SaveManager.Instance.LoadGame();
            }
            else
            {
                Debug.LogWarning("[CONTINUE] Không tìm thấy file save để tiếp tục!");
            }
        }
        else
        {
            Debug.LogError("[CONTINUE ERROR] Không tìm thấy Singleton SaveManager.Instance!");
        }
    }
}