using UnityEngine;

public class BossShoot : MonoBehaviour
{
    [Header("Bullet Prefab")]
    public GameObject bulletPrefab;

    [Header("Fire Point")]
    public Transform firePoint;

    [Header("Boss Move")]
    public BossAnimatorController boss;

    [HideInInspector]
    public bool isUsingShoot;

    private bool canShoot = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Chạm: " + other.name);

        if (!canShoot)
            return;

        if (other.CompareTag("Player"))
        {
            StartShoot();
        }
    }

    public void StartShoot()
    {
        canShoot = false;
        isUsingShoot = true;

        if (boss != null)
        {
            boss.isCastingLaser = true;
            boss.PlayShoot();
        }
    }

    // Animation Event giữa animation
    public void SpawnBullet()
    {
        Debug.Log("SpawnBullet được gọi");

        if (bulletPrefab == null || firePoint == null)
            return;

        Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );
    }

    // Animation Event cuối animation
    public void FinishShoot()
    {
        isUsingShoot = false;
        canShoot = true;

        if (boss != null)
        {
            boss.isCastingLaser = false;
        }
    }
}