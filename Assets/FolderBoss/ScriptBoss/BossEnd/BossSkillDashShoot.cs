using UnityEngine;
using System.Collections;

public class BossSkillDashShoot : MonoBehaviour
{
    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = .4f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Spread")]
    public int bulletsPerShot = 7;
    public float spreadAngle = 90f;

    BossEndController controller;
    Rigidbody2D rb;

    Transform player;

    bool isCasting;

    //--------------------------------

    void Awake()
    {
        controller =
        GetComponent<BossEndController>();

        rb =
        GetComponent<Rigidbody2D>();
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

        isCasting = true;

        controller.LockMovement(
            true
        );

        //--------------------------------
        // khóa vị trí player
        //--------------------------------

        Vector2 lockPos =
        player.position;

        Vector2 dashDir =
        (
            lockPos -
            (Vector2)
            transform.position
        ).normalized;

        //--------------------------------

        controller.PlayAttack();

        //--------------------------------
        // DASH
        //--------------------------------

        float timer =
        dashDuration;

        while (timer > 0)
        {
            rb.MovePosition(
                rb.position +
                dashDir *
                dashSpeed *
                Time.deltaTime
            );

            timer -=
            Time.deltaTime;

            yield return null;
        }

        //--------------------------------

        rb.linearVelocity =
        Vector2.zero;

        //--------------------------------
        // bắn
        //--------------------------------

        Shoot(
            dashDir
        );

        //--------------------------------

        controller.PlayIdle();

        controller.LockMovement(
            false
        );

        isCasting =
        false;
    }

    //--------------------------------

    void Shoot(
        Vector2 dir
    )
    {
        float center =
        Mathf.Atan2(
            dir.y,
            dir.x
        )
        *
        Mathf.Rad2Deg;

        float start =
        center -
        spreadAngle * .5f;

        float step =
        spreadAngle /
        (
            bulletsPerShot - 1
        );

        for (
            int i = 0;
            i < bulletsPerShot;
            i++
        )
        {
            float angle =
            start +
            step * i;

            Quaternion rot =
            Quaternion.Euler(
                0,
                0,
                angle
            );

            Instantiate(
                bulletPrefab,
                firePoint.position,
                rot
            );
        }
    }
}