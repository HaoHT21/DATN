using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SaveFileName = "savegame.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasSave()
    {
        return File.Exists(SavePath);
    }

    // =========================================================
    // SAVE GAME
    // =========================================================
    public void SaveGame()
    {
        if (isLoading)
        {
            Debug.LogWarning("[SAVE] Đang trong quá trình Load, không thể thực hiện Save!");
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("[SAVE ERROR] Không tìm thấy GameObject có Tag 'Player'!");
            return;
        }

        PlayerController player = playerObject.GetComponent<PlayerController>();
        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();

        if (player == null || health == null)
        {
            Debug.LogError("[SAVE ERROR] Player thiếu Component PlayerController hoặc PlayerHealth!");
            return;
        }

        SavesData data = new SavesData();

        // 1. Scene & Vị trí
        data.sceneName = SceneManager.GetActiveScene().name;
        Vector3 pos = playerObject.transform.position;

        data.playerX = pos.x;
        data.playerY = pos.y;
        data.playerZ = pos.z;

        data.playerPosX = pos.x;
        data.playerPosY = pos.y;
        data.playerPosZ = pos.z;

        // 2. Máu / Mana / Level / EXP (Chuyển đổi kiểu dữ liệu an toàn)
        data.currentHealth = health.currentHealth;
        data.maxHealth = health.maxHealth;
        data.currentMana = health.currentMana;
        data.maxMana = health.maxMana;

        data.currentLevel = (int)health.currentLevel;
        data.currentEXP = (float)health.currentEXP;
        data.maxLevel = (int)health.maxLevel;

        // 3. Coins (Tiền)
        if (PlayerStats.Instance != null)
        {
            data.coins = (int)PlayerStats.Instance.Score;
            data.score = (int)PlayerStats.Instance.Score;
        }

        // 4. Hero ID & Hero sở hữu (Giữ chính xác ID Hero)
        data.selectedHeroID = FindCurrentHeroID(playerObject);
        data.ownedHeroIDs = HeroSlot.GetOwnedHeroIDs();

        // 5. Kho đồ / Vũ khí (Lưu chi tiết từng ID vũ khí)
        data.inventory = new List<SavedWeaponData>();
        if (InventoryData.Instance != null && InventoryData.Instance.sharedInventory != null)
        {
            foreach (PlayerController.WeaponItem weapon in InventoryData.Instance.sharedInventory)
            {
                if (weapon == null) continue;

                SavedWeaponData savedWeapon = new SavedWeaponData
                {
                    itemID = weapon.itemID,
                    isGun = weapon.isGun,
                    damage = weapon.damage,
                    isPotion = weapon.isPotion,
                    healAmount = weapon.healAmount
                };

                data.inventory.Add(savedWeapon);
            }

            data.currentWeaponIndex = InventoryData.Instance.currentWeaponIndex;
        }

        if (data.inventory.Count == 0)
        {
            data.currentWeaponIndex = 0;
        }
        else
        {
            data.currentWeaponIndex = Mathf.Clamp(data.currentWeaponIndex, 0, data.inventory.Count - 1);
        }

        data.savedAtUtcIso = DateTime.UtcNow.ToString("o");

        // Ghi dữ liệu ra File JSON
        string json = JsonUtility.ToJson(data, true);
        try
        {
            File.WriteAllText(SavePath, json);
            Debug.Log($"<color=green>[SAVE THÀNH CÔNG]</color> File: {SavePath}\n" +
                      $"Hero ID: {data.selectedHeroID} | Pos: ({data.playerPosX:F1}, {data.playerPosY:F1}) | HP: {data.currentHealth}/{data.maxHealth} | Coins: {data.coins}");
        }
        catch (Exception e)
        {
            Debug.LogError("[SAVE ERROR] Không thể ghi file JSON: " + e.Message);
        }
    }

    // =========================================================
    // LOAD GAME / CONTINUE
    // =========================================================
    public void LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[LOAD] Chưa có file Save để Load!");
            return;
        }

        if (isLoading) return;

        SavesData data = ReadSave();
        if (data == null) return;

        isLoading = true;
        StartCoroutine(LoadGameRoutine(data));
    }

    private SavesData ReadSave()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogError("[LOAD ERROR] File save không tồn tại!");
                return null;
            }

            string json = File.ReadAllText(SavePath);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[LOAD ERROR] File Save bị trống!");
                return null;
            }

            SavesData data = JsonUtility.FromJson<SavesData>(json);

            // Fallback key cũ
            if (data.playerPosX == 0 && data.playerX != 0) data.playerPosX = data.playerX;
            if (data.playerPosY == 0 && data.playerY != 0) data.playerPosY = data.playerY;
            if (data.playerPosZ == 0 && data.playerZ != 0) data.playerPosZ = data.playerZ;
            if (data.coins == 0 && data.score != 0) data.coins = data.score;

            Debug.Log($"<color=yellow>[READ SAVE SUCCESS]</color> Đọc được Hero ID: {data.selectedHeroID} | Pos: ({data.playerPosX:F1}, {data.playerPosY:F1})");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("[LOAD ERROR] Lỗi khi đọc File Save: " + e.Message);
            return null;
        }
    }

    private IEnumerator LoadGameRoutine(SavesData data)
    {
        if (!string.IsNullOrEmpty(data.sceneName) && SceneManager.GetActiveScene().name != data.sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(data.sceneName);
            while (!operation.isDone)
            {
                yield return null;
            }
        }

        yield return new WaitForEndOfFrame();

        Vector3 targetPosition = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
        GameObject playerObject = ReplacePlayerWithSavedHero(data.selectedHeroID, targetPosition);

        if (playerObject == null)
        {
            Debug.LogError("[LOAD ERROR] Không thể khởi tạo Player!");
            isLoading = false;
            yield break;
        }

        // Tắt tạm Collider để tránh chạm phải TriggerZone NPC ban đầu khi dịch chuyển
        Collider2D playerCollider = playerObject.GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = false;

        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

        yield return new WaitForFixedUpdate();

        RestorePlayerData(playerObject, data);

        if (rb != null)
        {
            rb.position = new Vector2(data.playerPosX, data.playerPosY);
            rb.linearVelocity = Vector2.zero;
            rb.simulated = true;
        }
        playerObject.transform.position = targetPosition;

        // Bật lại Collider sau khi đã di chuyển đến đúng vị trí lưu
        if (playerCollider != null) playerCollider.enabled = true;

        UpdateCameraFollow(playerObject);

        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            try { inventoryUI.UpdateUI(); } catch { }
        }

        isLoading = false;
        Debug.Log($"<color=cyan>[LOAD THÀNH CÔNG]</color> Hero ID: {data.selectedHeroID} | Pos: {targetPosition}");
    }

    private GameObject ReplacePlayerWithSavedHero(int heroID, Vector3 spawnPosition)
    {
        GameObject oldPlayer = GameObject.FindGameObjectWithTag("Player");

        GameObject heroPrefab = null;
        if (HiroManager.Instance != null)
        {
            heroPrefab = HiroManager.Instance.GetHeroPrefabByID(heroID);
        }

        if (heroPrefab == null && HiroManager.Instance != null && HiroManager.Instance.allHeroes != null)
        {
            foreach (var h in HiroManager.Instance.allHeroes)
            {
                if (h != null && h.itemID == heroID)
                {
                    heroPrefab = h.heroPrefab;
                    break;
                }
            }

            if (heroPrefab == null && HiroManager.Instance.allHeroes.Count > 0)
            {
                heroPrefab = HiroManager.Instance.allHeroes[0].heroPrefab;
            }
        }

        if (heroPrefab == null)
        {
            if (oldPlayer != null)
            {
                oldPlayer.transform.position = spawnPosition;
                return oldPlayer;
            }
            return null;
        }

        GameObject newPlayer = Instantiate(heroPrefab, spawnPosition, Quaternion.identity);
        newPlayer.tag = "Player";

        PlayerController pc = newPlayer.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.heroID = heroID;
        }

        if (oldPlayer != null && oldPlayer != newPlayer)
        {
            oldPlayer.tag = "Untagged";
            Destroy(oldPlayer);
        }

        return newPlayer;
    }

    private void RestorePlayerData(GameObject playerObject, SavesData data)
    {
        if (playerObject == null) return;

        Vector3 savedPosition = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
        playerObject.transform.position = savedPosition;

        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();
        if (health != null)
        {
            // Ép kiểu (int) triệt để tránh lỗi CS0266
            health.maxHealth = data.maxHealth > 0 ? (int)data.maxHealth : health.maxHealth;
            health.currentHealth = (int)Mathf.Clamp(data.currentHealth, 0, health.maxHealth);

            health.maxMana = data.maxMana > 0 ? (int)data.maxMana : health.maxMana;
            health.currentMana = (int)Mathf.Clamp(data.currentMana, 0, health.maxMana);

            health.currentLevel = data.currentLevel;
            health.currentEXP = (int)data.currentEXP;
            health.maxLevel = data.maxLevel;

            try { health.SendMessage("UpdateUI", SendMessageOptions.DontRequireReceiver); } catch { }
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.Score = data.coins;
            try { PlayerStats.Instance.UpdateUI(); } catch { }
        }

        HeroSlot.LoadOwnedHeroIDs(data.ownedHeroIDs);
        RestoreInventory(playerObject.GetComponent<PlayerController>(), data);
    }

    private void RestoreInventory(PlayerController player, SavesData data)
    {
        if (InventoryData.Instance == null) return;

        // Xóa sạch vũ khí cũ trong kho
        InventoryData.Instance.sharedInventory.Clear();

        // Xóa toàn bộ Visual Vũ Khí cũ đang gắn trên tay Player để tránh bị đè visual
        if (player != null && player.weaponHolder != null)
        {
            foreach (Transform child in player.weaponHolder)
            {
                Destroy(child.gameObject);
            }
        }

        if (player == null || data.inventory == null || data.inventory.Count == 0) return;

        for (int i = 0; i < data.inventory.Count; i++)
        {
            SavedWeaponData saved = data.inventory[i];
            if (saved == null) continue;

            ItemData itemData = null;
            if (ItemDatabase.Instance != null)
            {
                itemData = ItemDatabase.Instance.GetItemByID(saved.itemID);
            }

            if (itemData == null)
            {
                Debug.LogError($"[LOAD INVENTORY ERROR] Không tìm thấy ItemData cho ID: {saved.itemID} trong ItemDatabase! Hãy kiểm tra lại Inspector của ItemDatabase.");
                continue;
            }

            GameObject visual = null;

            // Khởi tạo Prefab hiển thị vũ khí trên tay
            if (itemData.visualPrefab != null && player.weaponHolder != null)
            {
                visual = Instantiate(itemData.visualPrefab, player.weaponHolder);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = itemData.visualPrefab.transform.localScale;

                // Dọn dẹp component thừa của item nhặt ngoài đất
                ItemPickup pickup = visual.GetComponent<ItemPickup>();
                if (pickup != null) Destroy(pickup);

                Collider2D col = visual.GetComponent<Collider2D>();
                if (col != null) Destroy(col);

                Rigidbody2D visualRB = visual.GetComponent<Rigidbody2D>();
                if (visualRB != null) Destroy(visualRB);

                CollectibleSaveable saveable = visual.GetComponent<CollectibleSaveable>();
                if (saveable != null) Destroy(saveable);

                // Bật Visual nếu đây là vũ khí đang cầm trên tay
                bool isCurrentWeapon = (i == data.currentWeaponIndex);
                visual.SetActive(isCurrentWeapon);

                // Gán firePoint cho Player nếu đây là cây súng active hiện tại
                if (isCurrentWeapon && saved.isGun)
                {
                    Transform fp = visual.transform.Find("FirePoint");
                    if (fp != null)
                    {
                        player.firePoint = fp;
                    }
                    else
                    {
                        player.firePoint = visual.transform; // Fallback lấy gốc của visual
                    }
                }
            }

            PlayerController.WeaponItem weapon = new PlayerController.WeaponItem
            {
                itemID = saved.itemID,
                icon = itemData.itemIcon,
                visualPrefab = visual,
                pickupPrefab = itemData.itemPrefab,
                isGun = saved.isGun,
                damage = (int)saved.damage,
                bulletPrefab = itemData.bulletPrefab, // Gán bulletPrefab trực tiếp từ ItemData
                isPotion = saved.isPotion,
                healAmount = (int)saved.healAmount
            };

            InventoryData.Instance.sharedInventory.Add(weapon);
        }

        // Cập nhật lại chỉ số vũ khí active
        InventoryData.Instance.currentWeaponIndex = Mathf.Clamp(
            data.currentWeaponIndex,
            0,
            Mathf.Max(0, InventoryData.Instance.sharedInventory.Count - 1)
        );

        // Cập nhật lại giao diện và visual vũ khí cho Player
        player.UpdateWeaponVisuals();
    }

    private int FindCurrentHeroID(GameObject playerObject)
    {
        if (playerObject == null) return 20;

        PlayerController player = playerObject.GetComponent<PlayerController>();
        if (player != null && player.heroID >= 20 && player.heroID <= 23)
        {
            return player.heroID;
        }

        if (HiroManager.Instance != null && HiroManager.Instance.allHeroes != null)
        {
            string playerName = playerObject.name.Replace("(Clone)", "").Trim();

            foreach (HeroData hero in HiroManager.Instance.allHeroes)
            {
                if (hero == null || hero.heroPrefab == null) continue;

                string prefabName = hero.heroPrefab.name.Replace("(Clone)", "").Trim();

                if (playerName.Equals(prefabName, StringComparison.OrdinalIgnoreCase))
                {
                    return hero.itemID;
                }
            }
        }

        return 20;
    }

    private void UpdateCameraFollow(GameObject playerObject)
    {
        if (playerObject == null) return;

        var camera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (camera != null)
        {
            camera.Follow = playerObject.transform;
            camera.LookAt = playerObject.transform;
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[SAVE] Đã xóa thành công File Save.");
        }
    }
}