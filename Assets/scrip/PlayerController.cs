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

    [System.Serializable]
    public class WeaponItem
    {
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
        FixFirePointPosition();
    }

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (moveInput.x != 0)
        {
            _sprite.flipX = moveInput.x < 0;
            if (weaponHolder != null)
            {
                // XOAY VŨ KHÍ THEO HƯỚNG NHÂN VẬT
                weaponHolder.localRotation = Quaternion.Euler(0, 0, !_sprite.flipX ? weaponRotationOffset : 180f - weaponRotationOffset);

                // SỬA TẠI ĐÂY: Lấy đúng Scale hiện tại mày cấu hình tay ngoài Inspector để lật mặt, ĐÉO ÉP VỀ 0.5 NỮA!
                Vector3 currentScale = weaponHolder.localScale;
                float absX = Mathf.Abs(currentScale.x);
                float absY = Mathf.Abs(currentScale.y);
                float absZ = Mathf.Abs(currentScale.z);

                weaponHolder.localScale = new Vector3(absX, !_sprite.flipX ? absY : -absY, absZ);
            }
            FixFirePointPosition();
        }

        _animator.SetBool("isWalking", moveInput.magnitude > 0.1f);
        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.K) && _attackTimer <= 0 && inventory != null && inventory.Count > 0) PerformAttack();

        if (Input.GetKeyDown(KeyCode.R) && inventory != null && inventory.Count > 1)
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % inventory.Count;
            UpdateWeaponVisuals();
            FixFirePointPosition();
        }

        if (Input.GetKeyDown(KeyCode.T) && inventory != null && inventory.Count > 0) DropWeapon();
    }

    private void FixedUpdate() => rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);

    public void PerformAttack()
    {
        if (inventory == null || inventory.Count == 0) return;
        var weapon = inventory[currentWeaponIndex];
        _attackTimer = attackRate;

        if (weapon.isGun && weapon.bulletPrefab != null)
        {
            if (AudioManager.Instance != null && shootSound != null)
                AudioManager.Instance.PlaySound(shootSound);

            Quaternion rot = _sprite.flipX ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;
            GameObject b = Instantiate(weapon.bulletPrefab, firePoint.position, rot);
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
                inventory[i].visualPrefab.SetActive(i == currentWeaponIndex);
            }
        }
        OnInventoryChanged?.Invoke();
    }

    public void PickupWeapon(GameObject visualPrefab, GameObject pickupPrefab, bool isGun, int dmg, GameObject bulletType, Sprite icon)
    {
        if (inventory == null) return;
        if (inventory.Count >= 4) return;

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

            // Giữ nguyên Scale của cây súng lúc tạo ra
            spawned.transform.localScale = visualPrefab.transform.localScale;

            spawned.SetActive(false);
        }

        inventory.Add(new WeaponItem
        {
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
        UpdateWeaponVisuals();
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

    void FixFirePointPosition()
    {
        if (firePoint != null) { firePoint.localPosition = new Vector3(firePointXOffset, firePointYOffset, 0); firePoint.localRotation = Quaternion.identity; }
    }
}