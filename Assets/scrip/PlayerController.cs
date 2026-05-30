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
        public bool isPotion;    // Thêm để nhận diện bình máu
        public int healAmount;   // Lượng máu hồi
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
        if (!GameplayInputGate.CanProcessInput)
        {
            moveInput = Vector2.zero;
            _animator.SetBool("isWalking", false);
            return;
        }

        // 1. Logic di chuyển
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        if (moveInput.x != 0)
        {
            bool isFlip = moveInput.x < 0;
            _sprite.flipX = isFlip;

            if (weaponHolder != null)
            {
                float s = 0.2f;
                if (!isFlip)
                {
                    weaponHolder.localRotation = Quaternion.Euler(0, 0, weaponRotationOffset);
                    weaponHolder.localScale = new Vector3(s, s, s);
                }
                else
                {
                    weaponHolder.localRotation = Quaternion.Euler(0, 0, 180f - weaponRotationOffset);
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
        if (!GameplayInputGate.CanProcessInput)
            return;

        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    public void PerformAttack()
    {
        var weapon = inventory[_currentWeaponIndex];
        _attackTimer = attackRate;

        if (weapon.isPotion)
        {
            // DÙNG BÌNH MÁU
            PlayerHealth ph = GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.Heal(weapon.healAmount);

                // --- SỬA Ở ĐÂY: XÓA CẢ HÌNH ẢNH TRÊN TAY ---
                if (weapon.visualPrefab != null)
                {
                    Destroy(weapon.visualPrefab);
                }
                // -------------------------------------------

                inventory.RemoveAt(_currentWeaponIndex);
                _currentWeaponIndex = Mathf.Clamp(_currentWeaponIndex, 0, Mathf.Max(0, inventory.Count - 1));

                // Cập nhật lại UI/Visuals sau khi xóa
                UpdateWeaponVisuals();
            }
        }
        else if (weapon.isGun && weapon.bulletPrefab != null)
        {
            // BẮN SÚNG (Giữ nguyên như cũ)
            Quaternion bulletRotation = _sprite.flipX ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;
            GameObject bulletObj = Instantiate(weapon.bulletPrefab, firePoint.position, bulletRotation);
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = weapon.damage;
            }
        }
        else
        {
            _animator.SetTrigger("Attack");
        }
    }

    public void PickupWeapon(GameObject visualPrefab, GameObject pickupPrefab, bool isGun, int dmg, GameObject bulletType)
    {
        if (inventory.Count >= MAX_INVENTORY_SIZE) return;
        if (pickupPrefab == null) return;

        GameObject spawned = Instantiate(visualPrefab, weaponHolder);
        spawned.transform.localPosition = Vector3.zero;
        spawned.transform.localRotation = Quaternion.identity;
        spawned.transform.localScale = new Vector3(2f, 2f, 2f);
        spawned.SetActive(false);

        WeaponItem newItem = new WeaponItem
        {
            visualPrefab = spawned,
            pickupPrefab = pickupPrefab,
            isGun = isGun,
            damage = dmg,
            bulletPrefab = bulletType,
            isPotion = false
        };

        inventory.Add(newItem);
        _currentWeaponIndex = inventory.Count - 1;
        UpdateWeaponVisuals();
    }

    void DropWeapon()
    {
        if (inventory == null || inventory.Count == 0) return;
        var item = inventory[_currentWeaponIndex];

        // Không vứt bình máu nếu không có pickupPrefab
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
}