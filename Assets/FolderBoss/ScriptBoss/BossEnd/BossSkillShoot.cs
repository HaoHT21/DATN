using UnityEngine;
using System.Collections;

public class BossSkillShoot : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Shoot")]
    public int burstCount = 5;
    public float shootDelay = .25f;

    [Header("Spread")]
    public int bulletsPerShot = 5;
    public float spreadAngle = 60f;

    private BossEndController controller;

    bool isCasting;

    //--------------------------------

    void Awake()
    {
        controller =
        GetComponent<BossEndController>();
    }

    //--------------------------------

    public bool IsCasting()
    {
        return isCasting;
    }

    //--------------------------------

    public IEnumerator Cast()
    {
        isCasting = true;

        controller.LockMovement(
            true
        );

        for (
            int i = 0;
            i < burstCount;
            i++
        )
        {
            controller.PlayAttack();

            yield return
            new WaitForSeconds(
                .25f
            );

            Shoot();

            yield return
            new WaitForSeconds(
                shootDelay
            );
        }

        controller.PlayIdle();

        controller.LockMovement(
            false
        );

        isCasting = false;
    }

    //--------------------------------

    void Shoot()
    {
        if (
            bulletPrefab == null ||
            firePoint == null
        )
            return;

        float startAngle =
        -spreadAngle * .5f;

        float step =
        bulletsPerShot > 1
        ?
        spreadAngle /
        (bulletsPerShot - 1)
        :
        0;

        for (
            int i = 0;
            i < bulletsPerShot;
            i++
        )
        {
            float angle =
            startAngle +
            step * i;

            Quaternion rot =
            firePoint.rotation *
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