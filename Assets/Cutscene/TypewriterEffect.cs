using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text textComponent;
    public float timePerCharacter = 0.05f;
    private string fullText; // Biến lưu trữ văn bản gốc

    void Awake()
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();

        // Bắt buộc lưu văn bản gốc lại trước
        fullText = textComponent.text;
    }

    // Hàm này sẽ được Signal gọi
    public void StartTypewriter()
    {
        gameObject.SetActive(true);
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();

        // Nếu fullText bị rỗng (do gọi muộn), lấy lại text hiện tại
        if (string.IsNullOrEmpty(fullText))
        {
            fullText = textComponent.text;
        }

        StopAllCoroutines();
        StartCoroutine(ShowTextRoutine());
    }

    private IEnumerator ShowTextRoutine()
    {
        textComponent.text = ""; // Xóa chữ để bắt đầu gõ từng ký tự

        foreach (char c in fullText.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(timePerCharacter);
        }
    }
}