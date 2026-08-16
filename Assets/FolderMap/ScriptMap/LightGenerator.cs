using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LightGenerator : MonoBehaviour
{
    [Header("UI Progress (Filled Image)")]
    public Canvas progressCanvas;      // Canvas WorldSpace chứa UI tiến độ
    public Image progressFillImage;    // UI Image đặt Image Type = Filled (Radial hoặc Horizontal)

    [Header("Thời gian sửa")]
    public float repairDuration = 3f;  // Số giây cần giữ phím E để sửa xong
    public KeyCode interactKey = KeyCode.E;

    [HideInInspector] public bool isRepaired = false;
    [HideInInspector] public RoomLightManager manager;

    private bool isPlayerNearby = false;
    private float currentRepairProgress = 0f;

    private void Start()
    {
        if (progressCanvas != null)
            progressCanvas.gameObject.SetActive(false);

        if (progressFillImage != null)
        {
            progressFillImage.type = Image.Type.Filled; // Bắt buộc đặt kiểu Filled
            progressFillImage.fillAmount = 0f;
        }
    }

    private void Update()
    {
        if (isRepaired || !isPlayerNearby) return;

        // Player nhấn giữ phím E
        if (Input.GetKey(interactKey))
        {
            currentRepairProgress += Time.deltaTime;

            if (progressCanvas != null && !progressCanvas.gameObject.activeSelf)
                progressCanvas.gameObject.SetActive(true);

            // Cập nhật giá trị fillAmount từ 0.0f đến 1.0f
            if (progressFillImage != null)
                progressFillImage.fillAmount = Mathf.Clamp01(currentRepairProgress / repairDuration);

            if (currentRepairProgress >= repairDuration)
            {
                CompleteRepair();
            }
        }
        else
        {
            // Thả phím E ra thì reset tiến độ
            if (currentRepairProgress > 0f)
            {
                ResetProgressUI();
            }
        }
    }

    private void CompleteRepair()
    {
        isRepaired = true;
        currentRepairProgress = repairDuration;

        if (progressFillImage != null)
            progressFillImage.fillAmount = 1f;

        if (progressCanvas != null)
            progressCanvas.gameObject.SetActive(false);

        Debug.Log($"[LightGenerator] Máy phát sáng {gameObject.name} đã được sửa xong!");

        if (manager != null)
        {
            manager.OnGeneratorRepaired(this);
        }
    }

    public void ResetGenerator()
    {
        isRepaired = false;
        ResetProgressUI();
    }

    private void ResetProgressUI()
    {
        currentRepairProgress = 0f;
        if (progressFillImage != null) progressFillImage.fillAmount = 0f;
        if (progressCanvas != null) progressCanvas.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isRepaired)
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            ResetProgressUI();
        }
    }
}