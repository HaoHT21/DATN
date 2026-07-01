using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform target;        // Player
    public float rotateSpeed = 5f;  // tốc độ xoay

    void Start()
    {
        // Tự tìm Player theo Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        // hướng từ object -> player
        Vector3 direction = target.position - transform.position;

        // tính góc quay (2D)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation;

        // xoay object theo hướng player
        // nhìn sang trái
        if (direction.x < 0)
        {
            targetRotation = Quaternion.Euler(180f, 0f, -angle);
        }
        else
        {
            targetRotation = Quaternion.Euler(0f, 0f, angle);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}