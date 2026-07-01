using System.Collections;
using UnityEngine;

public class BulletLaser : MonoBehaviour
{
    [Header("Time")]
    public float aimTime = 1f;      // Thời gian ngắm
    public float showTime = 1f;     // Hiện object trong bao lâu

    [Header("Object")]
    public GameObject targetObject; // Object sẽ hiện sau khi khóa hướng

    [Header("Delay")]
    public float spawnDelay = .5f;

    private Transform player;
    private bool lockRotation;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;

        // Ẩn object lúc đầu
        if (targetObject != null)
            targetObject.SetActive(false);

        StartCoroutine(LockRoutine());
    }

    void Update()
    {
        if (lockRotation)
            return;

        if (player == null)
            return;

        Vector2 direction = player.position - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    IEnumerator LockRoutine()
    {
        //--------------------------------
        // Ngắm player
        //--------------------------------

        yield return new WaitForSeconds(
            aimTime
        );

        //--------------------------------
        // Khóa hướng hiện tại
        //--------------------------------

        lockRotation = true;

        //--------------------------------
        // Delay trước khi bắn
        //--------------------------------

        yield return new WaitForSeconds(
            spawnDelay
        );

        //--------------------------------
        // Bắn
        //--------------------------------

        if (targetObject != null)
            targetObject.SetActive(
                true
            );

        //--------------------------------
        // Giữ laser
        //--------------------------------

        yield return new WaitForSeconds(
            showTime
        );

        //--------------------------------
        // Hủy
        //--------------------------------

        Destroy(
            gameObject
        );
    }
}
