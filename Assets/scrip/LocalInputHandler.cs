using UnityEngine;

public class LocalInputHandler : MonoBehaviour
{
    // Script này có thể được gán vào Player để xử lý Input thay vì dùng Network Input Data
    public Vector2 GetMovementInput()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public bool IsAttackPressed() => Input.GetButtonDown("Fire1");
    public bool IsRPressed() => Input.GetKeyDown(KeyCode.R);
}