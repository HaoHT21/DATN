using UnityEngine;

public class BossLookAtPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform bossVisual;

    Transform player;

    void Start()
    {
        GameObject p =
        GameObject.FindGameObjectWithTag(
            "Player"
        );

        if (p != null)
        {
            player =
            p.transform;
        }
    }

    void Update()
    {
        if (
        player == null ||
        bossVisual == null
        )
            return;

        Flip();
    }

    void Flip()
    {
        Vector3 scale =
        bossVisual.localScale;

        if (
        player.position.x >
        transform.position.x
        )
        {
            scale.x =
            Mathf.Abs(
                scale.x
            );
        }
        else
        {
            scale.x =
            -Mathf.Abs(
                scale.x
            );
        }

        bossVisual.localScale =
        scale;
    }
}