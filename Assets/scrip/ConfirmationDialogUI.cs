using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hộp thoại xác nhận tái sử dụng (Có / Không).
/// </summary>
public class ConfirmationDialogUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;
    private Action _onCancel;

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        if (confirmButton != null)
            confirmButton.onClick.AddListener(HandleConfirm);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(HandleCancel);

        Hide();
    }

    public void Show(string message, Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (messageText != null)
            messageText.text = message;

        root.SetActive(true);
    }

    public void Hide()
    {
        _onConfirm = null;
        _onCancel = null;
        root.SetActive(false);
    }

    public bool IsVisible => root != null && root.activeSelf;

    private void HandleConfirm()
    {
        Action callback = _onConfirm;
        Hide();
        callback?.Invoke();
    }

    private void HandleCancel()
    {
        Action callback = _onCancel;
        Hide();
        callback?.Invoke();
    }
}
