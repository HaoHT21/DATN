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

    [Tooltip("Chỉnh góc này để nòng súng nằm ngang (Thường là -45)")]
    public float weaponRotationOffset = -45f;

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
        public GameObject visualPrefab;
        public GameObject pickupPrefab;
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

        // XỬ LÝ LẬT NGƯỜI VÀ XOAY SÚNG
        if (moveInput.x != 0)
        {
            bool isFlip = moveInput.x < 0;
            _sprite.flipX = isFlip;

            if (weaponHolder != null)
            {
                // ÉP CỨNG TỈ LỆ 0.2 CHO HOLDER
                float s = 0.2f;

                if (!isFlip) // Nhìn sang PHẢI
                {
                    weaponHolder.localRotation = Quaternion.Euler(0, 0, weaponRotationOffset);
                    weaponHolder.localScale = new Vector3(s, s, s);
                }
                else // Nhìn sang TRÁI
                {
                    // Xoay 180 độ và bù trừ offset
                    weaponHolder.localRotation = Quaternion.Euler(0, 0, 180f - weaponRotationOffset);
                    // Lật súng theo trục Y để không ngửa bụng
                    weaponHolder.localScale = new Vector3(s, -s, s);
                }
            }
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
        if (pickupPrefab == null) return;

        GameObject spawned = Instantiate(visualPrefab, weaponHolder);

        spawned.transform.localPosition = Vector3.zero;
        spawned.transform.localRotation = Quaternion.identity;

        // ÉP CỨNG TỈ LỆ 0.2 CHO SÚNG CON (CLONE)
        spawned.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

        spawned.SetActive(false);

        WeaponItem newItem = new WeaponItem
        {
            visualPrefab = spawned,
            pickupPrefab = pickupPrefab,
            isGun = isGun,
            damage = dmg,
            bulletPrefab = bulletType
        };

        inventory.Add(newItem);
        _currentWeaponIndex = inventory.Count - 1;
        UpdateWeaponVisuals();
    }

    void DropWeapon()
    {
        if (inventory == null || inventory.Count == 0) return;
        var item = inventory[_currentWeaponIndex];
        if (item.pickupPrefab != null)
            Instantiate(item.pickupPrefab, transform.position + (Vector3.down * 0.5f), Quaternion.identity);
        if (item.visualPrefab != null) Destroy(item.visualPrefab);
        inventory.RemoveAt(_currentWeaponIndex);
        _currentWeaponIndex = Mathf.Clamp(_currentWeaponIndex, 0, Mathf.Max(0, inventory.Count - 1));
        UpdateWeaponVisuals();
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
            Quaternion bulletRotation = _sprite.flipX ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;

            // --- SỬA Ở ĐÂY ---
            GameObject bulletObj = Instantiate(weapon.bulletPrefab, firePoint.position, bulletRotation);

            // Lấy script Bullet và gán sát thương vào
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = weapon.damage; // Truyền damage từ vũ khí vào đạn
            }
            // ------------------
        }
        else
        {
            _animator.SetTrigger("Attack");
        }
    }
}