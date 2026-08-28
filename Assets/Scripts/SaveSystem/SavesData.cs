using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavesData
{
    public string sceneName;

    // Vị trí Player
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;

    // Biến tương thích cũ (Fallback)
    public float playerX;
    public float playerY;
    public float playerZ;

    // Chỉ số sinh tồn (dùng float/int linh hoạt cho PlayerHealth)
    public float currentHealth;
    public float maxHealth;
    public float currentMana;
    public float maxMana;

    // Cấp độ & Kinh nghiệm
    public int currentLevel;
    public float currentEXP;
    public int maxLevel;

    // Tiền / Score
    public int coins;
    public int score;

    // Hero ID & Hero đã sở hữu
    public int selectedHeroID;
    public List<int> ownedHeroIDs = new List<int>();

    // Kho đồ & Vũ khí
    public List<SavedWeaponData> inventory = new List<SavedWeaponData>();
    public int currentWeaponIndex;

    // Thời gian Save ISO
    public string savedAtUtcIso;
}

[Serializable]
public class SavedWeaponData
{
    public int itemID;
    public bool isGun;
    public float damage;
    public bool isPotion;
    public float healAmount;
}