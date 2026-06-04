using UnityEngine;

public class NPCShop : MonoBehaviour
{
    public RectTransform shopPanel;
    public GameObject interactionCanvas;
    private bool _canOpen;

    private void Update()
    {
        if (_canOpen && Input.GetKeyDown(KeyCode.E))
        {
            bool open = !shopPanel.gameObject.activeSelf;
            shopPanel.gameObject.SetActive(open);
            interactionCanvas.SetActive(!open);
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) { _canOpen = true; interactionCanvas.SetActive(true); } }
    private void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) { _canOpen = false; interactionCanvas.SetActive(false); shopPanel.gameObject.SetActive(false); } }
}