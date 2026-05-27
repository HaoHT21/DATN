using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float attackRate = 0.5f;
    public Transform weaponHolder;
    public Transform firePoint;

    [Header("Inventory")]
    public List<WeaponItem> inventory = new List<WeaponItem>();
    private const int MAX_INVENTORY_SIZE = 4;

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float _attackTimer;
    private int _currentWeaponIndex = 0;

    [System.Serializable]
    public class WeaponItem
    {
        public GameObject visualPrefab; // Bản sao trên tay
        public GameObject pickupPrefab; // File Prefab gốc
        public bool isGun;
        public int damage;
        public GameObject bulletPrefab;
    }

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
        // 1. Logic di chuyển
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        if (moveInput.x != 0)
        {
            _sprite.flipX = moveInput.x < 0;
            if (weaponHolder != null)
                weaponHolder.localRotation = Quaternion.Euler(0, _sprite.flipX ? 180 : 0, 0);
        }

        _animator.SetBool("isWalking", moveInput.magnitude > 0.1f);

        // 2. Logic chiến đấu
        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.K) && _attackTimer <= 0 && inventory.Count > 0) PerformAttack();

        // 3. Logic đổi súng
        if (Input.GetKeyDown(KeyCode.R) && inventory.Count > 1)
        {
            _currentWeaponIndex = (_currentWeaponIndex + 1) % inventory.Count;
            UpdateWeaponVisuals();
        }

        // 4. Logic vứt súng
        if (Input.GetKeyDown(KeyCode.E) && inventory.Count > 0) DropWeapon();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    public void PickupWeapon(GameObject visualPrefab, GameObject pickupPrefab, bool isGun, int dmg, GameObject bulletType)
    {
        if (inventory.Count >= MAX_INVENTORY_SIZE) return;

        // KIỂM TRA ĐẦU VÀO
        if (pickupPrefab == null)
        {
            Debug.LogError("PickupWeapon: pickupPrefab bị null, không thể nhặt!");
            return;
        }

        // Tạo visual trên tay
        GameObject spawned = Instantiate(visualPrefab, weaponHolder);
        spawned.transform.localPosition = Vector3.zero;
        spawned.transform.localRotation = Quaternion.identity;
        spawned.SetActive(false);

        // THÊM VÀO LIST - Lưu ý: pickupPrefab ở đây PHẢI là file gốc trong Project
        WeaponItem newItem = new WeaponItem();
        newItem.visualPrefab = spawned;
        newItem.pickupPrefab = pickupPrefab;
        newItem.isGun = isGun;
        newItem.damage = dmg;
        newItem.bulletPrefab = bulletType;

        inventory.Add(newItem);

        Debug.Log($"Đã nhặt thành công: {pickupPrefab.name}. Index: {inventory.Count - 1}");

        _currentWeaponIndex = inventory.Count - 1;
        UpdateWeaponVisuals();
    }

    void DropWeapon()
    {
        Debug.Log("--- BẮT ĐẦU VỨT SÚNG ---");

        if (inventory == null || inventory.Count == 0)
        {
            Debug.Log("LỖI: Inventory rỗng hoặc null!");
            return;
        }

        Debug.Log($"Đang vứt súng tại index: {_currentWeaponIndex}. Tổng số súng: {inventory.Count}");
        var item = inventory[_currentWeaponIndex];

        if (item == null)
        {
            Debug.Log("LỖI: Item tại index này là null!");
            return;
        }

        if (item.pickupPrefab != null)
        {
            Debug.Log("Đang thực hiện Instantiate...");
            GameObject dropped = Instantiate(item.pickupPrefab, transform.position + (Vector3.down * 0.5f), Quaternion.identity);

            if (dropped != null)
            {
                Debug.Log($"THÀNH CÔNG: Súng {dropped.name} đã được tạo!");
            }
            else
            {
                Debug.Log("LỖI: Instantiate trả về null!");
            }
        }
        else
        {
            Debug.Log("LỖI: item.pickupPrefab là null, không thể Instantiate!");
        }

        if (item.visualPrefab != null) Destroy(item.visualPrefab);
        inventory.RemoveAt(_currentWeaponIndex);
        _currentWeaponIndex = Mathf.Clamp(_currentWeaponIndex, 0, Mathf.Max(0, inventory.Count - 1));
        UpdateWeaponVisuals();
        Debug.Log("--- KẾT THÚC VỨT SÚNG ---");
    }

    void UpdateWeaponVisuals()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].visualPrefab != null)
                inventory[i].visualPrefab.SetActive(i == _currentWeaponIndex);
        }
    }

    void PerformAttack()
    {
        var weapon = inventory[_currentWeaponIndex];
        _attackTimer = attackRate;

        if (weapon.isGun && weapon.bulletPrefab != null)
        {
            GameObject b = Instantiate(weapon.bulletPrefab, firePoint.position, firePoint.rotation);
            if (b.TryGetComponent<Bullet>(out var bullet)) bullet.damage = weapon.damage;
        }
        else
        {
            _animator.SetTrigger("Attack");
        }
    }
}