using UnityEngine;
using Fusion;

public class ChestNetwork : MonoBehaviour
{
    [Header("Animator của hộp")]
    [SerializeField]
    private Animator anim;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Có object chạm trigger");

        // Kiểm tra tag Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player đã chạm hộp");

            // Chạy animation
            if (anim != null)
            {
                anim.SetTrigger("Open");

                Debug.Log("Đã chạy animation");
            }
            else
            {
                Debug.LogError("Chưa gắn Animator");
            }
        }
    }
}