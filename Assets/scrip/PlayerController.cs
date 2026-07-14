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
    public AudioClip healSound;
    public AudioClip dropSound;

    [Header("Visual")]
    public Transform visual;

    [Header("Avatar")]
    public Sprite playerAvatar;

    public bool isKnocked = false;


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

    //private float gunScaleValue = 0.5f;



    [Header("Ép vị trí đầu nòng")]

    [SerializeField] private float firePointXOffset = 0.355f;

    [SerializeField] private float firePointYOffset = 0.353f;



    private void Awake()

    {

        _animator = GetComponent<Animator>();

        _sprite = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0;

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        rb.freezeRotation = true;

        EnsureWeaponMounts();

    }

    /// <summary>
    /// Bảo đảm weaponHolder + firePoint còn hợp lệ.
    /// FirePoint/FP là child của WeaponHolder — không được xóa khi clear visual súng.
    /// </summary>
    public void EnsureWeaponMounts()
    {
        if (weaponHolder == null)
        {
            Transform foundHolder = transform.Find("WeaponHolder");
            if (foundHolder != null)
                weaponHolder = foundHolder;
        }

        if (firePoint != null)
            return;

        Transform foundFirePoint = null;
        if (weaponHolder != null)
        {
            foundFirePoint = weaponHolder.Find("FirePoint");
            if (foundFirePoint == null)
                foundFirePoint = weaponHolder.Find("FP");
        }

        if (foundFirePoint == null)
            foundFirePoint = transform.Find("FirePoint") ?? transform.Find("FP");

        if (foundFirePoint == null)
            foundFirePoint = FindDeepChild(transform, "FirePoint") ?? FindDeepChild(transform, "FP");

        if (foundFirePoint != null)
        {
            firePoint = foundFirePoint;
            return;
        }

        // Nếu đã bị Destroy trước đó — tạo lại dưới WeaponHolder.
        Transform parent = weaponHolder != null ? weaponHolder : transform;
        var go = new GameObject("FirePoint");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(firePointXOffset, firePointYOffset, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        firePoint = go.transform;
    }

    /// <summary>
    /// Xóa visual vũ khí dưới WeaponHolder, giữ lại FirePoint/FP.
    /// </summary>
    public void ClearWeaponVisualsKeepMounts()
    {
        EnsureWeaponMounts();
        if (weaponHolder == null)
            return;

        for (int i = weaponHolder.childCount - 1; i >= 0; i--)
        {
            Transform child = weaponHolder.GetChild(i);
            if (IsPersistentWeaponMount(child))
                continue;

            Destroy(child.gameObject);
        }
    }

    private bool IsPersistentWeaponMount(Transform child)
    {
        if (child == null)
            return true;

        if (firePoint != null && child == firePoint)
            return true;

        string n = child.name;
        return n == "FirePoint" || n == "FP";
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null)
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
                return child;

            Transform nested = FindDeepChild(child, childName);
            if (nested != null)
                return nested;
        }

        return null;
    }



    private void Update()

    {
        if (!GameplayInputGate.CanProcessInput)
        {
            moveInput = Vector2.zero;
            _animator.SetBool("isWalking", false);
            return;
        }

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

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

            //if (visual != null)

            //{

            //    Vector3 scale = visual.localScale;

            //    scale.x = _sprite.flipX ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);

            //    visual.localScale = scale;

            //}

            //if (weaponHolder != null)

            //{

            //    float s = gunScaleValue;

            //    //weaponHolder.localRotation = Quaternion.Euler(0, 0, !_sprite.flipX ? weaponRotationOffset : 180f - weaponRotationOffset);

            //    //weaponHolder.localScale = new Vector3(s, !_sprite.flipX ? s : -s, s);

            //}

            //FixFirePointPosition();

        }



        _animator.SetBool("isWalking", moveInput.magnitude > 0.1f);

        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;



        if (Input.GetKeyDown(KeyCode.K) && _attackTimer <= 0 && inventory != null && inventory.Count > 0) PerformAttack();



        if (Input.GetKeyDown(KeyCode.R) && inventory != null && inventory.Count > 1)

        {

            currentWeaponIndex = (currentWeaponIndex + 1) % inventory.Count;

            UpdateWeaponVisuals();

            //FixFirePointPosition();

        }



        if (Input.GetKeyDown(KeyCode.T) && inventory != null && inventory.Count > 0) DropWeapon();

    }



    private void FixedUpdate()
    {
        if (isKnocked || !GameplayInputGate.CanProcessInput)
            return;

        rb.MovePosition(
            rb.position +
            moveInput *
            moveSpeed *
            Time.fixedDeltaTime
        );
    }



    public void PerformAttack()

    {

        if (inventory == null || inventory.Count == 0) return;

        var weapon = inventory[currentWeaponIndex];

        _attackTimer = attackRate;



        if (weapon.isGun && weapon.bulletPrefab != null)

        {
            EnsureWeaponMounts();
            if (firePoint == null)
                return;

            if (AudioManager.Instance != null && shootSound != null)

                AudioManager.Instance.PlaySound(shootSound);



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

            if (ph != null) { ph.Heal(weapon.healAmount); RemoveWeapon(currentWeaponIndex); }

        }

        else { _animator.SetTrigger("Attack"); }

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

            // Prefab súng thường tự trỏ chính nó; cần bỏ collider/pickup và giữ renderer bật.
            SanitizeHeldWeaponVisual(spawned);

            // Mặc định tắt — UpdateWeaponVisuals() sẽ bật lại nếu đang được chọn.
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



        // Gọi hàm này để cập nhật trạng thái SetActive(true) cho vũ khí vừa nhặt!

        UpdateWeaponVisuals();

    }

    private static void SanitizeHeldWeaponVisual(GameObject spawned)
    {
        if (spawned == null)
            return;

        foreach (CollectibleSaveable c in spawned.GetComponentsInChildren<CollectibleSaveable>(true))
            Destroy(c);

        foreach (SaveableEntity c in spawned.GetComponentsInChildren<SaveableEntity>(true))
            Destroy(c);

        foreach (ItemPickup c in spawned.GetComponentsInChildren<ItemPickup>(true))
            Destroy(c);

        foreach (HealthPotion c in spawned.GetComponentsInChildren<HealthPotion>(true))
            Destroy(c);

        foreach (Collider2D c in spawned.GetComponentsInChildren<Collider2D>(true))
            Destroy(c);

        foreach (Collider c in spawned.GetComponentsInChildren<Collider>(true))
            Destroy(c);

        SpriteRenderer[] sprites = spawned.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                sprites[i].enabled = true;
        }

        Renderer[] renderers = spawned.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = true;
        }
    }

    void DropWeapon()

    {

        if (inventory == null || inventory.Count == 0) return;

        var item = inventory[currentWeaponIndex];

        if (item.pickupPrefab) Instantiate(item.pickupPrefab, transform.position + Vector3.down, Quaternion.identity);

        RemoveWeapon(currentWeaponIndex);

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

    public void DropWeaponAtSlot(int slotIndex)
    {
        if (inventory == null)
            return;

        if (slotIndex < 0 || slotIndex >= inventory.Count)
            return;

        WeaponItem item = inventory[slotIndex];

        // Sinh item rơi xuống đất
        if (item.pickupPrefab != null)
        {
            Instantiate(
                item.pickupPrefab,
                transform.position + Vector3.down,
                Quaternion.identity);
        }

        // Xóa model trên tay
        if (item.visualPrefab != null)
        {
            Destroy(item.visualPrefab);
        }

        inventory.RemoveAt(slotIndex);

        // Nếu xóa món đang chọn hoặc trước món đang chọn
        if (currentWeaponIndex >= inventory.Count)
        {
            currentWeaponIndex =
                Mathf.Max(0, inventory.Count - 1);
        }

        UpdateWeaponVisuals();
    }



    //void FixFirePointPosition()

    //{

    //    if (firePoint != null) { firePoint.localPosition = new Vector3(firePointXOffset, firePointYOffset, 0); firePoint.localRotation = Quaternion.identity; }

    //}

}