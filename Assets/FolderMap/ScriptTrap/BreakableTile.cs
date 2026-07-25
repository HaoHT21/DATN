using UnityEngine;

public class BreakableTile : MonoBehaviour
{
    public int hp = 1;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}