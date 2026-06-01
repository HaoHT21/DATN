using UnityEngine;

public class ShopInteraction : MonoBehaviour
{
    public GameObject interactUI;
    public GameObject shopPanel;
    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Kiểm tra shopPanel trước khi thao tác
            if (shopPanel != null)
            {
                shopPanel.SetActive(!shopPanel.activeSelf);
            }
            else
            {
                Debug.LogError("ShopPanel bị mất liên kết! Hãy kiểm tra Inspector của NPC.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactUI != null) interactUI.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
        }
    }
}