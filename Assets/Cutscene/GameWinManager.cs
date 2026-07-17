using UnityEngine;
using UnityEngine.UI;

public class GameWinManager : MonoBehaviour
{
    public static GameWinManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject creditPanel;

    [Header("UI Components")]
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (winPanel != null) winPanel.SetActive(false);
        if (creditPanel != null) creditPanel.SetActive(false);
    }

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonPressed);
        }
    }

    public void OnBossDeath()
    {
        if (winPanel != null) winPanel.SetActive(true);
    }

    private void OnContinueButtonPressed()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (creditPanel != null) creditPanel.SetActive(true);
    }
}