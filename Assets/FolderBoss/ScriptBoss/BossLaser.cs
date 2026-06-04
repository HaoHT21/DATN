using UnityEngine;

public class BossLaser : MonoBehaviour
{
    [Header("Laser Object")]
    public GameObject laserObject;

    [Header("Boss Move")]
    public BossAnimatorController boss;

    [HideInInspector]
    public bool isUsingLaser;

    private Transform player;

    private bool canUseLaser = true;

    private void Start()
    {
        if (laserObject != null)
            laserObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canUseLaser)
            return;

        if (other.CompareTag("Player"))
        {
            StartLaser();
        }
    }

    public void StartLaser()
    {
        canUseLaser = false;
        isUsingLaser = true;

        if (boss != null)
        {
            boss.isCastingLaser = true;
            boss.PlayLaserCast();
        }

        ShowLaser();
    }

    public void ShowLaser()
    {
        if (laserObject != null)
            laserObject.SetActive(true);
    }

    public void HideLaser()
    {
        if (laserObject != null)
            laserObject.SetActive(false);

        canUseLaser = true;
        isUsingLaser = false;

        if (boss != null)
        {
            boss.isCastingLaser = false;
            Debug.Log("Boss được bật lại");
        }
    }
}