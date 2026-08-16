using UnityEngine;

public class PlayerStealth : MonoBehaviour
{
    public bool IsHidden { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bush"))
        {
            IsHidden = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Bush"))
        {
            IsHidden = false;
        }
    }
}