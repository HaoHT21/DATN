using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Cài đặt cơ bản")]
    public float moveSpeed = 5f;
    public float attackRate = 0.5f;
    public Transform weaponHolder;
    public Transform firePoint;
    public float weaponRotationOffset = -45f;


    [Header("Âm thanh")]
    public AudioClip shootSound;
    [Range(0f, 100f)] public float shootVolume = 100f; // Thanh trượt chỉnh âm lượng bắn súng từ 0 đến 100 ngoài Inspector
    public AudioClip healSound;
    public AudioClip dropSound;

    [Header("Visual")]
    public Transform visual;

    [Header("Avatar")]
    public Sprite playerAvatar;

    public bool isKnocked = false;

    [Header("Status")]
    public bool reverseControl = false;


    [System.Serializable]
    public class WeaponItem
    {
        public int itemID;
        public Sprite icon;
        public GameObject visualPrefab;
        public GameObject pickupPrefab;
        public bool isGun;
        public int damage;
        public GameObject bulletPrefab;
        public bool isPotion;
        public int healAmount;
    }

    // Kết nối tới kho đồ dùng chung
    public List<WeaponItem> inventory
    {
        get
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogError("LỖI: Chưa tìm thấy InventoryData Instance trong Scene! Hãy chắc chắn script InventoryData đã được gắn vào một GameObject.");
                return null;
            }
            return InventoryData.Instance.sharedInventory;
        }
    }

    public int currentWeaponIndex
    {
        get => InventoryData.Instance != null ? InventoryData.Instance.currentWeaponIndex : 0;
        set { if (InventoryData.Instance != null) InventoryData.Instance.currentWeaponIndex = value; }
    }

    public event Action OnInventoryChanged;

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float _attackTimer;

    public bool isFrozen = false; // Thêm biến này

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
    }

    private void Update()

    {

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (reverseControl)
        {
            moveInput = -moveInput;
        }

        if (moveInput.x != 0)
        {
            _sprite.flipX = moveInput.x < 0;
            if (moveInput.x != 0)
            {
                if (moveInput.x > 0)
                {
                    visual.rotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    visual.rotation = Quaternion.Euler(0, 180, 0);
                }
            }
        }

        _animator.SetBool("isWalking", moveInput.magnitude > 0.1f);

        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.K) && !isFrozen && _attackTimer <= 0 && inventory != null && inventory.Count > 0)
        {
            PerformAttack();
        }

        if (Input.GetKeyDown(KeyCode.R) && inventory != null && inventory.Count > 1)
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % inventory.Count;

            UpdateWeaponVisuals();
        }
        if (Input.GetKeyDown(KeyCode.T) && inventory != null && inventory.Count > 0) DropWeapon();
    }



    private void FixedUpdate()
    {
        // Nếu đang bị Knockback HOẶC bị Đóng băng thì không cho di chuyển
        if (isKnocked || isFrozen)
            return;

        rb.MovePosition(
            rb.position +
            moveInput *
            moveSpeed *
            moveFixedDeltaTime()
        );
    }

    private float moveFixedDeltaTime() => Time.fixedDeltaTime;



    public void PerformAttack()
    {
        // THÊM DÒNG NÀY: Ngăn chặn tấn công nếu đang bị đóng băng
        if (isFrozen) return;

        if (inventory == null || inventory.Count == 0) return;
        var weapon = inventory[currentWeaponIndex];
        _attackTimer = attackRate;

        if (weapon.isGun && weapon.bulletPrefab != null)
        {
            // CHỈ SỬA KHÚC NÀY: Khởi tạo AudioSource 2D thủ công để tiếng súng to rõ, không bị nhỏ do khoảng cách Camera
            if (shootSound != null)
            {
                GameObject tempAudio = new GameObject("TempShootAudio");
                tempAudio.transform.position = transform.position;
                AudioSource aSource = tempAudio.AddComponent<AudioSource>();

                aSource.clip = shootSound;
                aSource.spatialBlend = 0f; // Ép về âm thanh 2D hoàn toàn

                // Quy đổi mượt mà từ hệ 0-100 ngoài Inspector về hệ 0.0-1.0 chuẩn Unity
                aSource.volume = Mathf.Clamp01(shootVolume / 100f);

                // ==========================================
                // LONG MẠCH: Gán tiếng bắn súng đi qua đúng kênh CombatSFX của Audio Mixer
                if (AudioStaticManager.Instance != null)
                {
                    aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
                }
                // ==========================================

                aSource.Play();
                Destroy(tempAudio, shootSound.length); // Phát xong tự hủy Object tạm
            }

            GameObject b = Instantiate(
                weapon.bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );
            if (b.TryGetComponent<Bullet>(out var bs)) bs.damage = weapon.damage;
        }
        else if (weapon.isPotion)
        {
            PlayerHealth ph = GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.Heal(weapon.healAmount);
                // Xóa bình máu và cập nhật giao diện
                RemoveWeapon(currentWeaponIndex);
            }
        }
        else
        {
            // --- XỬ LÝ VŨ KHÍ CẬN CHIẾN (SWORD) ---
            _animator.SetTrigger(""); // Animation vung tay của Player (nếu có)

            if (weapon.visualPrefab != null)
            {
                // Lấy script nằm trên GameObject vũ khí đang cầm
                if (weapon.visualPrefab.TryGetComponent<SwordWeapon>(out var sword))
                {
                    sword.Attack();
                }
            }
        }
    }

    public void UpdateWeaponVisuals()

    {

        if (inventory == null) return;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].visualPrefab != null)
            {
                // Chỉ bật vũ khí đang cầm (trùng index), tắt toàn bộ vũ khí ẩn còn lại
                inventory[i].visualPrefab.SetActive(i == currentWeaponIndex);
            }

        }

        OnInventoryChanged?.Invoke();

    }



    public void PickupWeapon(GameObject visualPrefab, GameObject pickupPrefab, bool isGun, int dmg, GameObject bulletType, Sprite icon, int itemID = 0)
    {
        if (inventory == null) return;
        if (inventory.Count >= 20) return;

        if (weaponHolder == null)
        {
            Debug.LogError($"Lỗi: Bạn chưa kéo thả 'Weapon Holder' vào script PlayerController gắn trên {gameObject.name}!");
            return;
        }

        GameObject spawned = null;
        if (visualPrefab != null)
        {
            spawned = Instantiate(visualPrefab, weaponHolder);
            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = Quaternion.identity;
            spawned.transform.localScale = visualPrefab.transform.localScale;

            // =========================================================================
            // KHẮC PHỤC LỖI NHÂN BẢN:
            // Gỡ bỏ các Component nhặt đồ khỏi khẩu súng ĐANG CẦM TRÊN TAY
            // để nó không thể tự kích hoạt Trigger nhặt đồ hay va chạm vật lý nữa.
            // =========================================================================
            if (spawned.TryGetComponent<ItemPickup>(out var pickupScript))
                Destroy(pickupScript);

            if (spawned.TryGetComponent<Collider2D>(out var col))
                Destroy(col);

            if (spawned.TryGetComponent<Rigidbody2D>(out var rb))
                Destroy(rb);

            if (spawned.TryGetComponent<CollectibleSaveable>(out var saveable))
                Destroy(saveable);
            // =========================================================================

            spawned.SetActive(false);
        }

        inventory.Add(new WeaponItem
        {
            itemID = itemID,
            icon = icon,
            visualPrefab = spawned,
            pickupPrefab = pickupPrefab,
            isGun = isGun,
            damage = dmg,
            bulletPrefab = bulletType,
            isPotion = false,
            healAmount = 0
        });

        currentWeaponIndex = inventory.Count - 1;

        // Cập nhật hiển thị vũ khí cầm trên tay
        UpdateWeaponVisuals();
    }

    void RemoveWeapon(int index)

    {

        if (inventory == null || index < 0 || index >= inventory.Count) return;

        if (inventory[index].visualPrefab) Destroy(inventory[index].visualPrefab);

        inventory.RemoveAt(index);

        currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, Mathf.Max(0, inventory.Count - 1));

        UpdateWeaponVisuals();

    }



    public void SwitchToWeaponIndex(int slotIndex)
    {
        if (inventory == null)
            return;

        if (slotIndex < 0 || slotIndex >= inventory.Count)
            return;

        // --- THÊM PHẦN XỬ LÝ DÙNG BÌNH MÁU TẠI ĐÂY ---
        if (inventory[slotIndex].isPotion)
        {
            PlayerHealth ph = GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // Gọi hàm hồi máu của Player
                ph.Heal(inventory[slotIndex].healAmount);

                // Phát âm thanh hồi máu nếu có cấu hình
                if (AudioManager.Instance != null && healSound != null)
                {
                    AudioManager.Instance.PlaySound(healSound);
                }

                // Xóa bình máu ra khỏi danh sách
                RemoveWeapon(slotIndex);
            }
            return; // Ngăn chặn không cho gán làm vũ khí cầm trên tay
        }
        // ----------------------------------------------

        currentWeaponIndex = slotIndex;

        UpdateWeaponVisuals();
    }

    void DropWeapon()
    {
        if (inventory == null || inventory.Count == 0) return;
        DropWeaponAtSlot(currentWeaponIndex);
    }

    public void DropWeaponAtSlot(int slotIndex)
    {
        if (inventory == null || slotIndex < 0 || slotIndex >= inventory.Count) return;

        WeaponItem item = inventory[slotIndex];
        GameObject prefabToSpawn = item.pickupPrefab;

        // Tra cứu ItemDatabase nếu thiếu pickupPrefab
        if (prefabToSpawn == null && ItemDatabase.Instance != null)
        {
            ItemData data = ItemDatabase.Instance.GetItemByID(item.itemID);
            if (data != null)
            {
                prefabToSpawn = data.itemPrefab;
            }
        }

        // 1. Xóa model visual trên tay Player trước
        if (item.visualPrefab != null)
        {
            Destroy(item.visualPrefab);
        }

        // 2. Sinh item rơi xuống đất
        if (prefabToSpawn != null)
        {
            Vector3 dropDirection = _sprite.flipX ? Vector3.left : Vector3.right;
            Vector3 dropPosition = transform.position + dropDirection * 1f + Vector3.down * 0.2f;

            GameObject droppedItem = Instantiate(
                prefabToSpawn,
                dropPosition,
                Quaternion.identity
            );

            // Thêm lực văng nhẹ
            if (droppedItem.TryGetComponent<Rigidbody2D>(out var rbItem))
            {
                Vector2 force = new Vector2(dropDirection.x * 2f, 1.5f);
                rbItem.AddForce(force, ForceMode2D.Impulse);
            }
        }
        else
        {
            Debug.LogWarning($"[DropWeapon] Không tìm thấy Pickup Prefab cho ItemID: {item.itemID}");
        }

        // 3. Xóa khỏi danh sách Inventory
        inventory.RemoveAt(slotIndex);

        // Điều chỉnh lại index vũ khí hiện tại
        if (currentWeaponIndex >= inventory.Count)
        {
            currentWeaponIndex = Mathf.Max(0, inventory.Count - 1);
        }

        UpdateWeaponVisuals();
    }
}