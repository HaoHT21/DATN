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

    [Tooltip("Góc offset để súng nằm ngang")]
    public float weaponRotationOffset = -45f;

    [Header("Âm thanh")]
    public AudioClip shootSound;
    public AudioClip healSound;
    public AudioClip dropSound;

    [Header("Túi đồ")]
    public List<WeaponItem> inventory = new List<WeaponItem>();
    private const int MAX_INVENTORY_SIZE = 4;
    public int currentWeaponIndex { get; private set; } = 0;
    public event Action OnInventoryChanged;

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float _attackTimer;
    private float gunScaleValue = 0.5f;

    [Header("Ép vị trí đầu nòng")]
    [SerializeField] private float firePointXOffset = 0.355f;
    [SerializeField] private float firePointYOffset = 0.353f;

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
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        if (moveInput.x != 0)
        {
            bool isFlip = moveInput.x < 0;
            _sprite.flipX = isFlip;

            if (weaponHolder != null)
            {
                float s = gunScaleValue;
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
            FixFirePointPosition();
        }

        _animator.SetBool("isWalking", moveInput.magnitude > 0.1f);

        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.K) && _attackTimer <= 0 && inventory.Count > 0) PerformAttack();

        if (Input.GetKeyDown(KeyCode.R) && inventory.Count > 1)
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % inventory.Count;
            UpdateWeaponVisuals();
            FixFirePointPosition();
        }

        if (Input.GetKeyDown(KeyCode.T) && inventory.Count > 0) DropWeapon();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void FixFirePointPosition()
    {
        if (firePoint != null)
        {
            firePoint.localPosition = new Vector3(firePointXOffset, firePointYOffset, 0);
            firePoint.localScale = Vector3.one;
            firePoint.localRotation = Quaternion.identity;
        }
    }

    public void PerformAttack()
    {
        if (inventory.Count == 0) return;
        var weapon = inventory[currentWeaponIndex];
        _attackTimer = attackRate;

        if (weapon.isGun && weapon.bulletPrefab != null)
        {
            if (AudioManager.Instance != null && shootSound != null)
                AudioManager.Instance.PlaySound(shootSound);

            Quaternion bulletRotation = _sprite.flipX ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;
            GameObject bulletObj = Instantiate(weapon.bulletPrefab, firePoint.position, bulletRotation);

            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null) bulletScript.damage = weapon.damage;
        }
        else if (weapon.isPotion)
        {
            PlayerHealth ph = GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.Heal(weapon.healAmount);
                if (weapon.visualPrefab != null) Destroy(weapon.visualPrefab);
                inventory.RemoveAt(currentWeaponIndex);
                currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, Mathf.Max(0, inventory.Count - 1));
                UpdateWeaponVisuals();
                FixFirePointPosition();
            }
        }
        else
        {
            _animator.SetTrigger("Attack");
        }
    }

    public void UpdateWeaponVisuals()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].visualPrefab != null)
                inventory[i].visualPrefab.SetActive(i == currentWeaponIndex);
        }
        OnInventoryChanged?.Invoke();
    }

    public void PickupWeapon(GameObject visualPrefab, GameObject pickupPrefab, bool isGun, int dmg, GameObject bulletType, Sprite icon)
    {
        if (inventory.Count >= MAX_INVENTORY_SIZE) return;

        GameObject spawned = Instantiate(visualPrefab, weaponHolder);
        spawned.transform.localPosition = Vector3.zero;
        spawned.transform.localRotation = Quaternion.identity;
        spawned.transform.localScale = Vector3.one;

        inventory.Add(new WeaponItem
        {
            icon = icon,
            visualPrefab = spawned,
            pickupPrefab = pickupPrefab,
            isGun = isGun,
            damage = dmg,
            bulletPrefab = bulletType
        });

        currentWeaponIndex = inventory.Count - 1;
        UpdateWeaponVisuals();
        FixFirePointPosition();
    }

    void DropWeapon()
    {
        if (inventory.Count == 0) return;
        var item = inventory[currentWeaponIndex];

        if (item.pickupPrefab != null)
            Instantiate(item.pickupPrefab, transform.position + (Vector3.down * 0.5f), Quaternion.identity);

        if (item.visualPrefab != null) Destroy(item.visualPrefab);

        inventory.RemoveAt(currentWeaponIndex);
        currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, Mathf.Max(0, inventory.Count - 1));
        UpdateWeaponVisuals();
        FixFirePointPosition();
    }
}