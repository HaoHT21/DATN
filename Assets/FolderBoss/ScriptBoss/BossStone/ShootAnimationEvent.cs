using UnityEngine;

public class ShootAnimationEvent : MonoBehaviour
{
    public BossShoot shoot;

    private void Awake()
    {
        if (shoot == null)
            shoot = GetComponentInParent<BossShoot>();
    }

    // Event giữa animation
    public void SpawnBullet()
    {
        if (shoot != null)
            shoot.SpawnBullet();
    }

    // Event cuối animation
    public void FinishShoot()
    {
        if (shoot != null)
            shoot.FinishShoot();
    }
}