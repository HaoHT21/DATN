using UnityEngine;
using System.Collections;

public class GoddessMeteorSkill : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Bullet")]
    public GameObject bulletPrefab;

    public Transform firePoint;

    [Header("Rain")]

    [Tooltip("Tổng số đạn")]
    public int bulletCount = 20;

    [Tooltip("Thời gian giữa từng viên")]
    public float bulletInterval = .05f;

    [Tooltip("Độ lệch góc")]
    public float spreadAngle = 30f;

    BossEndController controller;

    //--------------------------------

    void Awake()
    {
        controller =
        GetComponent<BossEndController>();
    }

    //--------------------------------

    public IEnumerator Cast()
    {
        controller.PlayAttack();

        for (
            int i = 0;
            i < bulletCount;
            i++
        )
        {
            ShootBullet();

            yield return
            new WaitForSeconds(
                bulletInterval
            );
        }

        controller.PlayIdle();
    }

    //--------------------------------

    void ShootBullet()
    {
        if (
            bulletPrefab == null ||
            firePoint == null
        )
            return;

        //--------------------------------
        // random trong 30 độ
        //--------------------------------

        float angle =
        Random.Range(
            -spreadAngle * .5f,
            spreadAngle * .5f
        );

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
