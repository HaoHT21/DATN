using System.Collections;
using UnityEngine;

public class CameraColorZone : MonoBehaviour
{
    [Header("Cấu hình Màu sắc")]
    public Color targetColor = Color.black; // Màu nền mong muốn khi vào vùng này
    public float transitionDuration = 1.0f; // Thời gian chuyển màu (giây)

    [Header("Cấu hình Target")]
    public string playerTag = "Player";      // Tag của Player

    private Camera mainCamera;
    private Color defaultColor;
    private Coroutine colorChangeCoroutine;

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // Lưu lại màu nền ban đầu của Camera
            defaultColor = mainCamera.backgroundColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            ChangeCameraColor(targetColor);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Khi rời vùng, trả về màu mặc định ban đầu
            ChangeCameraColor(defaultColor);
        }
    }

    // Dùng cho game 3D (nếu dùng Collider 3D)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            ChangeCameraColor(targetColor);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            ChangeCameraColor(defaultColor);
        }
    }

    private void ChangeCameraColor(Color newColor)
    {
        if (mainCamera == null) return;

        // Nếu đang chạy Coroutine đổi màu cũ thì ngắt để chạy màu mới
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
        }

        colorChangeCoroutine = StartCoroutine(LerpColorRoutine(newColor));
    }

    private IEnumerator LerpColorRoutine(Color endColor)
    {
        Color startColor = mainCamera.backgroundColor;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            // Chuyển màu mượt mà theo thời gian
            mainCamera.backgroundColor = Color.Lerp(startColor, endColor, elapsed / transitionDuration);
            yield return null;
        }

        mainCamera.backgroundColor = endColor;
    }
}