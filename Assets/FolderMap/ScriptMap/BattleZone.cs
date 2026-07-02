using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    [Header("Wave")]
    public List<BattleWaveData> waves =
        new List<BattleWaveData>();

    [Header("Spawn")]
    public Collider2D spawnArea;

    [Header("Delay")]
    public float nextWaveDelay = 2f;

    private List<GameObject> aliveObjects =
        new List<GameObject>();

    private HashSet<Collider2D> playersInside =
        new HashSet<Collider2D>();

    private int currentWave = 0;

    private bool battleStarted = false;
    private bool battleCompleted = false;

    private Coroutine waveRoutine;

    //=====================
    // PLAYER ENTER
    //=====================

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (!other.CompareTag("Player"))
            return;

        if (battleCompleted)
            return;

        playersInside.Add(other);

        // Chỉ bắt đầu 1 lần
        if (battleStarted)
            return;

        battleStarted = true;

        waveRoutine =
            StartCoroutine(
                WaveRoutine()
            );

        Debug.Log("Battle Start");
    }

    //=====================
    // PLAYER EXIT
    //=====================

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        if (!other.CompareTag("Player"))
            return;

        playersInside.Remove(other);

        // Không còn player
        if (
            playersInside.Count <= 0
            &&
            !battleCompleted
        )
        {
            ResetBattle();
        }
    }

    //=====================
    // WAVE
    //=====================

    IEnumerator WaveRoutine()
    {
        while (
            currentWave <
            waves.Count
        )
        {
            BattleWaveData wave =
                waves[currentWave];

            SpawnWave(wave);

            yield return
            new WaitUntil(
                () => AllSpawnDead()
            );

            currentWave++;

            if (
                currentWave <
                waves.Count
            )
            {
                yield return
                new WaitForSeconds(
                    nextWaveDelay
                );
            }
        }

        BattleComplete();
    }

    //=====================
    // SPAWN
    //=====================

    void SpawnWave(
        BattleWaveData wave
    )
    {
        aliveObjects.Clear();

        for (
            int i = 0;
            i < wave.spawnCount;
            i++
        )
        {
            GameObject prefab =
                wave.spawnPrefabs[
                    Random.Range(
                        0,
                        wave.spawnPrefabs.Count
                    )
                ];

            GameObject obj =
                Instantiate(
                    prefab,
                    GetRandomPosition(),
                    Quaternion.identity
                );

            aliveObjects.Add(obj);
        }
    }

    //=====================
    // RANDOM POSITION
    //=====================

    Vector3 GetRandomPosition()
    {
        Bounds bounds =
            spawnArea.bounds;

        return new Vector3(
            Random.Range(
                bounds.min.x,
                bounds.max.x
            ),
            Random.Range(
                bounds.min.y,
                bounds.max.y
            ),
            0
        );
    }

    //=====================
    // CHECK DEAD
    //=====================

    bool AllSpawnDead()
    {
        aliveObjects.RemoveAll(
            x => x == null
        );

        return
        aliveObjects.Count <= 0;
    }

    //=====================
    // COMPLETE
    //=====================

    void BattleComplete()
    {
        battleCompleted = true;

        Debug.Log(
            "Battle Complete"
        );
    }

    //=====================
    // RESET
    //=====================

    public void ResetBattle()
    {
        if (
            battleCompleted
        )
            return;

        Debug.Log(
            "Reset Battle"
        );

        if (
            waveRoutine != null
        )
        {
            StopCoroutine(
                waveRoutine
            );
        }

        foreach (
            GameObject obj
            in aliveObjects
        )
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        aliveObjects.Clear();

        playersInside.Clear();

        currentWave = 0;

        battleStarted = false;
    }
}


[System.Serializable]
public class BattleWaveData
{
    [Header("Boss/Enemy")]
    public List<GameObject>
        spawnPrefabs =
        new List<GameObject>();

    [Header("Amount")]
    public int spawnCount = 5;
}