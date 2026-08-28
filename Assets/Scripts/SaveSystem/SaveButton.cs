using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SaveButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private float cooldownTime = 1.0f;
    private bool isSaving = false;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void SaveGame()
    {
        if (isSaving) return;

        if (SaveManager.Instance != null)
        {
            StartCoroutine(SaveRoutine());
        }
        else
        {
            Debug.LogError("[SAVE BUTTON] Không tìm thấy SaveManager!");
        }
    }

    private IEnumerator SaveRoutine()
    {
        isSaving = true;
        if (button != null) button.interactable = false;

        SaveManager.Instance.SaveGame();

        yield return new WaitForSecondsRealtime(cooldownTime);

        if (button != null) button.interactable = true;
        isSaving = false;
    }
}