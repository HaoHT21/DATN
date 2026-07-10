using UnityEngine;
using System.Collections;

public class BossSkillBulletRain : MonoBehaviour
{
    [Header("Spawn Area")]
    public float areaRadius = 6f;

    [Header("Bullet")]
    public GameObject bulletPrefab;

    public int bulletCount = 15;
    public float spawnDelay = .1f;

    [Header("Height")]
    public float spawnHeight = 4f;

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
            //--------------------------------
            // random quanh boss
            //--------------------------------

            Vector2 randomPos =
            (Vector2)
            transform.position
            +
            Random.insideUnitCircle
            *
            areaRadius;

            //--------------------------------
            // tạo trên cao
            //--------------------------------

            Vector2 spawnPos =
            new Vector2(
                randomPos.x,
                randomPos.y +
                spawnHeight
            );

            //--------------------------------

            Instantiate(
            bulletPrefab,
            spawnPos,
            Quaternion.identity
            );

            yield return
            new WaitForSeconds(
                spawnDelay
            );
        }

        controller.PlayIdle();
    }

    //--------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color =
        Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            areaRadius
        );
    }
}