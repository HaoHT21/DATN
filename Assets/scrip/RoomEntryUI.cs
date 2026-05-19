using UnityEngine;
using TMPro;

public class RoomEntryUI : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    private string _roomName;
    private string _password;

    // QUAN TRỌNG: Sửa tham số thứ 2 thành string thay vì Action
    public void Setup(string name, string password)
    {
        _roomName = name;
        _password = password;
        roomNameText.text = name;
    }

    // Hàm này được gọi khi nhấn nút Join trên dòng tên phòng
    public void OnClickJoin()
    {
        // Tìm MenuManager trong cảnh để yêu cầu hiện bảng nhập mật khẩu
        var menu = FindAnyObjectByType<NetworkMenuManager>();
        if (menu != null)
        {
            menu.ShowPasswordPrompt(_roomName, _password);
        }
    }
}