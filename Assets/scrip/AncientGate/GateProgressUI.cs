using System.Collections;
using TMPro;
using UnityEngine;

public class GateProgressUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject root;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI announcementText;

    [Header("Feedback")]
    public float gateOpenedMessageDuration = 3f;

    private Coroutine _announcementRoutine;

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        if (announcementText != null)
            announcementText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (GateManager.Instance != null)
            GateManager.Instance.OnProgressChanged += HandleProgressChanged;
    }

    private void OnDisable()
    {
        if (GateManager.Instance != null)
            GateManager.Instance.OnProgressChanged -= HandleProgressChanged;
    }

    public void SetProgress(int activated, int total)
    {
        if (progressText != null)
            progressText.text = $"Ngọc đã kích hoạt: {activated}/{total}";
    }

    public void ShowGateOpenedMessage()
    {
        if (_announcementRoutine != null)
            StopCoroutine(_announcementRoutine);

        _announcementRoutine = StartCoroutine(GateOpenedRoutine());
    }

    private void HandleProgressChanged(int activated, int total)
    {
        SetProgress(activated, total);
    }

    private IEnumerator GateOpenedRoutine()
    {
        if (announcementText != null)
        {
            announcementText.gameObject.SetActive(true);
            announcementText.text = "Cánh cổng đã mở!";
        }

        yield return new WaitForSeconds(gateOpenedMessageDuration);

        if (announcementText != null)
            announcementText.gameObject.SetActive(false);

        _announcementRoutine = null;
    }
}
