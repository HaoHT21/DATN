using UnityEngine;
using UnityEngine.EventSystems;


public class HDSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 5)]
    public string itemDescription;

    private UIManager uiManager;

    private void Awake()
    {
        uiManager = FindFirstObjectByType<UIManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (uiManager != null)
        {
            uiManager.ShowTooltip(itemDescription, Input.mousePosition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiManager != null)
        {
            uiManager.HideTooltip();
        }
    }

    private void OnDisable()
    {
        if (uiManager != null) uiManager.HideTooltip();
    }
}