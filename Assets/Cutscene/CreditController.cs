using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditController : MonoBehaviour
{
    [Header("Cấu hình")]
    [SerializeField] private float timeToReturnMenu = 12f; // Thời gian chạy hết Credit (bằng độ dài Animation)
    [SerializeField] private string menuSceneName = "MainMenu"; // Tên Scene Menu của bạn

    private void OnEnable()
    {
        // Khi CreditPanel được bật lên, bắt đầu đếm ngược thời gian kết thúc
        StartCoroutine(WaitAndReturnToMenu());
    }

    private IEnumerator WaitAndReturnToMenu()
    {
        yield return new WaitForSeconds(timeToReturnMenu);

        // Chuyển về màn hình chính
        Debug.Log("Credit kết thúc, quay về Menu...");
        // Khôi phục lại Time.timeScale nếu trước đó bạn có dừng game
        Time.timeScale = 1f;

        SceneManager.LoadScene(menuSceneName);
    }
}