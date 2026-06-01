using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    public GameObject heroShopPanel; // Kéo HeroShopPanel vào đây
    private bool isPlayerInRange;

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            heroShopPanel.SetActive(true); // Mở shop
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerInRange = false;
    }
}