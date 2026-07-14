using UnityEngine;

public class NPCInteraction1 : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactUI;

    public GameObject unfollowUI;

    private bool playerInRange;
    private NPCController controller;

    private void Awake()
    {
        controller = GetComponent<NPCController>();

        if (interactUI != null)
            interactUI.SetActive(false);

        if (unfollowUI != null)
            unfollowUI.SetActive(false);
    }

    private void Update()
    {
        UpdateUI();

        if (!playerInRange)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            NPCFollowerManager.Instance.SelectNPC(controller);

            // Cập nhật ngay sau khi đổi trạng thái
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (!playerInRange)
        {
            if (interactUI != null)
                interactUI.SetActive(false);

            if (unfollowUI != null)
                unfollowUI.SetActive(false);

            return;
        }

        bool isFollowing = NPCFollowerManager.Instance != null &&
                           NPCFollowerManager.Instance.currentFollower == controller;

        if (interactUI != null)
            interactUI.SetActive(!isFollowing);

        if (unfollowUI != null)
            unfollowUI.SetActive(isFollowing);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        UpdateUI();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        UpdateUI();
    }
}