using UnityEngine;
using System.Collections.Generic;

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

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float _attackTimer;
    private int _currentWeaponIndex = 0;

    private float gunScaleValue = 0.5f;

    [Header("Ép vị trí đầu nòng")]
    // Mình giữ biến này để bạn có thể chỉnh nhanh bên ngoài nếu cần
    [SerializeField] private float firePointXOffset = 0.355f;
    [SerializeField] private float firePointYOffset = 0.353f;

    [System.Serializable]
    public class WeaponItem
    {
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
            // Gọi Fix liên tục để đảm bảo vị trí luôn chuẩn khi xoay người
            FixFirePointPosition();
        }

        _animator.SetBool("isWalking", moveInput.magnitude > 0.1f);

        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.K) && _attackTimer <= 0 && inventory.Count > 0) PerformAttack();

        if (Input.GetKeyDown(KeyCode.R) && inventory.Count > 1)
        {
            _currentWeaponIndex = (_currentWeaponIndex + 1) % inventory.Count;
            UpdateWeaponVisuals();
            FixFirePointPosition();
        }

        if (Input.GetKeyDown(KeyCode.E) && inventory.Count > 0) DropWeapon();
    }

    private void FixedUpdate()
    {
        if (!GameplayInputGate.CanProcessInput)
            return;

        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    // HÀM QUAN TRỌNG NHẤT: Ép vị trí FirePoint
    void FixFirePointPosition()
    {
        if (firePoint != null)
        {
            // Ép chết tọa độ X và Y theo đúng vị trí sát nòng súng bạn muốn
            firePoint.localPosition = new Vector3(firePointXOffset, firePointYOffset, 0);

            firePoint.localScale = Vector3.one;
            firePoint.localRotation = Quaternion.identity;
        }
    }

    public void PerformAttack()
    {
        if (inventory.Count == 0) return;
        var weapon = inventory[_currentWeaponIndex];
        _attackTimer = attackRate;

        if (weapon.isGun && weapon.bulletPrefab != null)
        {
            if (AudioManager.Instance != null && shootSound != null)
                AudioManager.Instance.PlaySound(shootSound);

            Quaternion bulletRotation = _sprite.flipX ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;

            // Tạo đạn tại vị trí FirePoint đã được Fix
            GameObject bulletObj = Instantiate(weapon.bulletPrefab, firePoint.position, bulletRotation);

            // Gán sát thương cho đạn
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = weapon.damage;
            }
        }
        else if (weapon.isPotion)
        {
            PlayerHealth ph = GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.Heal(weapon.healAmount);
                if (weapon.visualPrefab != null) Destroy(weapon.visualPrefab);
                inventory.RemoveAt(_currentWeaponIndex);
                _currentWeaponIndex = Mathf.Clamp(_currentWeaponIndex, 0, Mathf.Max(0, inventory.Count - 1));
                UpdateWeaponVisuals();
                FixFirePointPosition();
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

        GameObject spawned = Instantiate(visualPrefab, weaponHolder);
        spawned.transform.localPosition = Vector3.zero;
        spawned.transform.localRotation = Quaternion.identity;
        spawned.transform.localScale = Vector3.one;
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
        FixFirePointPosition();
    }

    void DropWeapon()
    {
        if (inventory.Count == 0) return;
        var item = inventory[_currentWeaponIndex];
        if (item.pickupPrefab != null)
            Instantiate(item.pickupPrefab, transform.position + (Vector3.down * 0.5f), Quaternion.identity);

        if (item.visualPrefab != null) Destroy(item.visualPrefab);
        inventory.RemoveAt(_currentWeaponIndex);
        _currentWeaponIndex = Mathf.Clamp(_currentWeaponIndex, 0, Mathf.Max(0, inventory.Count - 1));
        UpdateWeaponVisuals();
        FixFirePointPosition();
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