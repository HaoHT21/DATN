using UnityEngine;
using System.Collections;

public class GoddessCloneSkill : MonoBehaviour
{
    [Header("Clone")]
    public GameObject clonePrefab;

    public int cloneCount = 3;

    public float cloneLifeTime = 30f;

    [Header("Spawn Area")]
    public float spawnRadius = 5f;

    public LayerMask wallLayer;

    public int maxTry = 10;

    //--------------------------------

    public IEnumerator Cast()
    {
        if (clonePrefab == null)
        {
            Debug.LogWarning(
                "Clone Skill thiếu setup"
            );

            yield break;
        }

        for (
            int i = 0;
            i < cloneCount;
            i++
        )
        {
            SpawnClone();

            yield return
            new WaitForSeconds(
                .2f
            );
        }
    }

    //--------------------------------

    void SpawnClone()
    {
        Vector2 spawnPos =
        FindValidPosition();

        GameObject clone =
        Instantiate(
            clonePrefab,
            spawnPos,
            Quaternion.identity
        );

        GodDessController goddess =
        clone.GetComponent<
            GodDessController
        >();

        if (goddess != null)
        {
            //--------------------------------
            // đánh dấu clone
            //--------------------------------

            goddess.isClone = true;

            //--------------------------------
            // clone không tạo clone
            //--------------------------------

            goddess.cloneSkill = null;
        }

        Destroy(
            clone,
            cloneLifeTime
        );
    }

    //--------------------------------

    Vector2 FindValidPosition()
    {
        Vector2 center =
        transform.position;

        //--------------------------------
        // thử nhiều lần
        //--------------------------------

        for (
            int i = 0;
            i < maxTry;
            i++
        )
        {
            Vector2 randomPos =
            center +
            Random.insideUnitCircle
            *
            spawnRadius;

            //--------------------------------
            // kiểm tra có tường không
            //--------------------------------

            Collider2D wall =
            Physics2D.OverlapCircle(
                randomPos,
                .5f,
                wallLayer
            );

            //--------------------------------
            // kiểm tra đường từ boss
            // tới vị trí spawn
            //--------------------------------

            RaycastHit2D hit =
            Physics2D.Linecast(
                center,
                randomPos,
                wallLayer
            );

            if (
                wall == null
                &&
                hit.collider == null
            )
            {
                return randomPos;
            }
        }

        //--------------------------------
        // nếu thử hết vẫn lỗi
        //--------------------------------

        return center;
    }

    //--------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color =
        Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            spawnRadius
        );
    }
}