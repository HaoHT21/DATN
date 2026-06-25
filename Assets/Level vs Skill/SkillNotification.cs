using System.Collections;
using UnityEngine;
using TMPro;

public class SkillNotification : MonoBehaviour
{
    // Khởi tạo Singleton để tất cả các script khác gọi nhanh không cần kéo thả
    public static SkillNotification Instance { get; private set; }

    [Header("--- Cấu hình Text Thông Báo ---")]
    public TextMeshProUGUI notificationText; // Kéo thả Object Text vào đây

    private Coroutine _hideCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Mới vào game thì ẩn chữ đi
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    // Hàm public thần thánh để các script khác gọi từ xa
    public void ShowMessage(string message, Color textColor)
    {
        if (notificationText == null) return;

        // Dừng Coroutine cũ nếu đang chạy để tránh đè chữ
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }

        notificationText.text = message;
        notificationText.color = textColor;
        notificationText.gameObject.SetActive(true);

        // Tự động ẩn chữ sau 2 giây
        _hideCoroutine = StartCoroutine(HideNotificationAfterDelay(2f));
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationText.gameObject.SetActive(false);
    }
}