using UnityEngine;
using System.Collections;

public class GoddessLaserSkill : MonoBehaviour
{
    [Header("Spawn Area")]
    public float areaRadius = 6f;

    public GameObject laserPrefab;

    [Header("Count")]
    public int laserCount = 6;

    public float spawnDelay = .3f;

    [Header("Laser")]
    public float laserDuration = 1.5f;

    Transform player;

    BossEndController controller;

    //--------------------------------

    void Awake()
    {
        controller =
        GetComponent<BossEndController>();

        GameObject obj =
        GameObject.FindGameObjectWithTag(
            "Player"
        );

        if (obj != null)
        {
            player =
            obj.transform;
        }
    }

    //--------------------------------

    public IEnumerator Cast()
    {
        controller.PlayAttack();

        for (
            int i = 0;
            i < laserCount;
            i++
        )
        {
            Vector2 pos =
            (Vector2)
            transform.position
            +
            Random.insideUnitCircle
            *
            areaRadius;

            StartCoroutine(
                SpawnLaser(
                    pos
                )
            );

            yield return
            new WaitForSeconds(
                spawnDelay
            );
        }

        controller.PlayIdle();
    }

    //--------------------------------

    IEnumerator SpawnLaser(
        Vector2 pos
    )
    {
        Vector2 dir =
        (
            player.position -
            (Vector3)pos
        ).normalized;

        float angle =
        Mathf.Atan2(
            dir.y,
            dir.x
        )
        *
        Mathf.Rad2Deg;

        //--------------------------------
        // tạo laser ngay
        //--------------------------------

        GameObject laser =
        Instantiate(
            laserPrefab,
            pos,
            Quaternion.Euler(
                0,
                0,
                angle
            )
        );

        yield return
        new WaitForSeconds(
            laserDuration
        );

        Destroy(
            laser
        );
    }

    //--------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color =
        Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            areaRadius
        );
    }
}