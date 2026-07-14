using System.Collections;
using UnityEngine;

public class DashSkill : NPCSkill
{
    [Header("Dash")]
    public float dashDistance = 3f;
    public float dashTime = 0.15f;

    public override void Use(GameObject player)
    {
        PlayerController controller =
            player.GetComponent<PlayerController>();

        if (controller == null)
            return;

        controller.StartCoroutine(Dash(controller));
    }

    IEnumerator Dash(PlayerController controller)
    {
        Rigidbody2D rb =
            controller.GetComponent<Rigidbody2D>();

        if (rb == null)
            yield break;

        //-----------------------------------
        // Hướng dash
        //-----------------------------------

        Vector2 direction = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        // Nếu không nhấn nút thì dash theo hướng đang nhìn
        if (direction == Vector2.zero)
        {
            SpriteRenderer sr =
                controller.GetComponent<SpriteRenderer>();

            direction =
                (sr != null && sr.flipX)
                ? Vector2.left
                : Vector2.right;
        }

        direction.Normalize();

        //-----------------------------------
        // Dash
        //-----------------------------------

        controller.isKnocked = true;

        Vector2 start = rb.position;
        Vector2 end = start + direction * dashDistance;

        float timer = 0;

        while (timer < dashTime)
        {
            timer += Time.fixedDeltaTime;

            rb.MovePosition(
                Vector2.Lerp(
                    start,
                    end,
                    timer / dashTime));

            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(end);

        controller.isKnocked = false;
    }
}