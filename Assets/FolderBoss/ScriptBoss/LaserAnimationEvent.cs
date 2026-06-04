using UnityEngine;

public class LaserAnimationEvent : MonoBehaviour
{
    public BossLaser laser;

    private void Awake()
    {
        if (laser == null)
            laser = GetComponentInParent<BossLaser>();
    }

    // Animation Event ở frame cuối
    public void FinishLaser()
    {
        if (laser != null)
            laser.ShowLaser();
    }
}