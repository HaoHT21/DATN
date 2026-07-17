using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện bắt buộc phải có để chuyển Scene

public class SceneController : MonoBehaviour
{
    // Hàm này sẽ được gán vào sự kiện Click của Button UI
    public void ExitToMainMenu()
    {
        // CỰC KỲ QUAN TRỌNG: Reset lại Time.timeScale về 1f 
        // Đề phòng trường hợp game đang Pause (đóng băng), nếu không load sang Menu sẽ bị đơ không click được gì.
        Time.timeScale = 1f;

        // Load Scene Menu chính. 
        // Hãy chắc chắn chữ "MainMenu" viết đúng chính tả từng ký tự giống tên Scene của tụi mày nhé!
        SceneManager.LoadScene("MainMenu");
    }
}