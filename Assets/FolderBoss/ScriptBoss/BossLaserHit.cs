using UnityEngine;

public class BossLaserHit : MonoBehaviour
{
    [Header("Laser Hitbox")]
    public GameObject laserHitbox;

    public BossLaser laser;

    private void Awake()
    {
        if (laser == null)
            laser = GetComponentInParent<BossLaser>();
    }

    private void Start()
    {
        if (laserHitbox != null)
            laserHitbox.SetActive(false);
    }

    // Event trong animation
    public void EnableHitbox()
    {
        if (laserHitbox != null)
            laserHitbox.SetActive(true);
    }

    // Event trong animation
    public void DisableHitbox()
    {
        if (laserHitbox != null)
            laserHitbox.SetActive(false);
    }

    // Event cuối animation
    public void FinishLaser()
    {
        if (laser != null)
            laser.HideLaser();
    }
}