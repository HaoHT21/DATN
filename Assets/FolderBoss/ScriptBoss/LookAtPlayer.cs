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

        // xoay object theo hướng player
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}