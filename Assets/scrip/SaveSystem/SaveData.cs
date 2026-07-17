using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class SaveData
{
    public int version = 1;

    public string sceneName;

    public float playerX;
    public float playerY;
    public float playerZ;

    public int score;

    public string savedAtUtcIso;

    public List<SaveRecord> records = new List<SaveRecord>();

    [Serializable]
    public sealed class SaveRecord
    {
        public string id;
        public string type;
        public string json;
    }

    public Vector3 GetPlayerPosition()
    {
        return new Vector3(playerX, playerY, playerZ);
    }

    public void SetPlayerPosition(Vector3 pos)
    {
        playerX = pos.x;
        playerY = pos.y;
        playerZ = pos.z;
    }
}

