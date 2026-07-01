using UnityEngine;
using System.Collections;

public class BossSkillTeleport : MonoBehaviour
{
    [Header("Teleport Area")]
    public float areaWidth = 8f;

    public float areaHeight = 6f;

    public int maxTry = 20;

    public LayerMask wallLayer;

    BossEndController controller;

    Transform player;

    //--------------------------------

    void Awake()
    {
        controller =
        GetComponent<BossEndController>();
    }

    //--------------------------------

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

    //--------------------------------

    public IEnumerator Cast()
    {
        if (player == null)
            yield break;

        controller.LockMovement(
            true
        );

        controller.PlayAttack();

        yield return
        new WaitForSeconds(
            .3f
        );

        //--------------------------------
        // tìm điểm hợp lệ
        //--------------------------------

        Vector2 teleportPos =
        FindTeleportPoint();

        //--------------------------------

        transform.position =
        teleportPos;

        //--------------------------------
        // random skill sau tele
        //--------------------------------

        int random =
        Random.Range(
            0,
            3
        );

        switch (random)
        {
            case 0:

                yield return
                StartCoroutine(
                    controller
                    .shootSkill
                    .Cast()
                );

                break;

            case 1:

                yield return
                StartCoroutine(
                    controller
                    .dashShootSkill
                    .Cast()
                );

                break;

            case 2:

                yield return
                StartCoroutine(
                    controller
                    .bulletRainSkill
                    .Cast()
                );

                break;
        }

        controller.LockMovement(
            false
        );
    }

    //--------------------------------

    Vector2 FindTeleportPoint()
    {
        for (
        int i = 0;
        i < maxTry;
        i++
        )
        {
            Vector2 point =
            new Vector2(
                player.position.x +
                Random.Range(
                    -areaWidth * .5f,
                    areaWidth * .5f
                ),

                player.position.y +
                Random.Range(
                    -areaHeight * .5f,
                    areaHeight * .5f
                )
            );

            //--------------------------------
            // kiểm tra nằm trong tường
            //--------------------------------

            Collider2D hitWall =
            Physics2D.OverlapCircle(
                point,
                .3f,
                wallLayer
            );

            if (hitWall)
                continue;

            //--------------------------------
            // kiểm tra dây xuyên tường
            //--------------------------------

            RaycastHit2D line =
            Physics2D.Linecast(
                transform.position,
                point,
                wallLayer
            );

            if (line.collider)
                continue;

            return point;
        }

        //--------------------------------
        // fallback
        //--------------------------------

        return transform.position;
    }

    //--------------------------------

    void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        Gizmos.color =
        Color.magenta;

        Gizmos.DrawWireCube(
            player.position,
            new Vector3(
                areaWidth,
                areaHeight,
                1
            )
        );
    }
}