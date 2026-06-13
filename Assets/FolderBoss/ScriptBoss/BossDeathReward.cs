using UnityEngine;
using System.Collections.Generic;

public class BossDeathReward : MonoBehaviour
{
    [Header("Tags cần tìm")]
    public string[] portalTags;

    private List<GameObject> portals =
        new List<GameObject>();

    private void Awake()
    {
        foreach (string tag in portalTags)
        {
            GameObject[] found =
                GameObject.FindGameObjectsWithTag(tag);

            portals.AddRange(found);
        }
    }

    private void Start()
    {
        foreach (GameObject portal in portals)
        {
            portal.SetActive(false);
        }
    }

    public void OnBossDeath()
    {
        foreach (GameObject portal in portals)
        {
            portal.SetActive(true);
        }
    }
}