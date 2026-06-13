using UnityEngine;
using System.Collections;

public class DirectAnimationController : MonoBehaviour
{
    private Animator _animator;
    private Transform _playerTransform;

    [Header("Tên các trạng thái trong Animator")]
    [SerializeField] private string introStateName = "B Animation";
    [SerializeField] private string idleStateName = "BIdel Animation";

    [Header("Cấu hình bắn đạn & Tốc độ")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int attackDamage = 10;
    public float bulletSpeed = 8f; // Tốc độ bay của viên đạn        

    [Space(5)]
    public bool loopShooting = true;
    public float fireRate = 1.5f;

    private bool _canShoot = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (firePoint == null)
        {
            firePoint = this.transform;
        }

        // Tự động định vị Player ngay từ đầu
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    private void Start()
    {
        // Ẩn UI lựa chọn nếu còn sót
        GameObject luaChonUI = GameObject.Find("LuaChon");
        if (luaChonUI != null)
        {
            luaChonUI.SetActive(false);
        }

        if (_animator != null)
        {
            StartCoroutine(PlaySequenceRoutine());
        }
    }

    private void Update()
    {
        // Liên tục kiểm tra để xoay mặt (Flip) nhân vật hướng về phía Player
        HandleLookAtPlayer();
    }

    // HÀM LẬT MẶT (FLIP) THEO VỊ TRÍ PLAYER
    private void HandleLookAtPlayer()
    {
        if (_playerTransform == null) return;

        // Nếu Player đang đứng bên trái NPC và NPC đang nhìn bên phải (hoặc ngược lại)
        if (_playerTransform.position.x < transform.position.x)
        {
            // Quay mặt sang trái (Góc trục Y = 180 độ)
            transform.rotation = Quaternion.Euler(0,0, 0);
        }
        else
        {
            // Quay mặt sang phải (Góc trục Y = 0 độ)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    IEnumerator PlaySequenceRoutine()
    {
        _animator.Play(introStateName);
        yield return new WaitForEndOfFrame();

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float playTime = stateInfo.length > 0 ? stateInfo.length : 1.5f;
        yield return new WaitForSeconds(playTime);

        _animator.Play(idleStateName);

        NPCInteraction interaction = GetComponent<NPCInteraction>();
        if (interaction != null)
        {
            float safetyTimer = 0f;
            while (!interaction.isFinished && safetyTimer < 15f)
            {
                safetyTimer += Time.deltaTime;
                yield return null;
            }
        }

        _canShoot = true;
        Debug.Log("🚀 [<color=green>DirectAnimationController</color>] Bắt đầu xả đạn hướng mục tiêu!");

        if (loopShooting)
        {
            while (true)
            {
                ShootBullet();
                yield return new WaitForSeconds(fireRate);
            }
        }
    }

    private void ShootBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 1. Tạo viên đạn
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // 2. Tính toán vector hướng bay
        Vector2 shootDirection = (_playerTransform != null)
            ? (_playerTransform.position - firePoint.position).normalized
            : (Vector2)transform.right;

        // 3. Xoay góc viên đạn
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 4. Ép lực vận tốc
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * bulletSpeed;
        }

        // 🛠️ SỬA ĐOẠN NÀY: Thay vì dùng SendMessage (dễ lỗi), hãy gán trực tiếp biến vào script
        NPCBullet bulletScript = bullet.GetComponent<NPCBullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = attackDamage;
        }
        else
        {
            Debug.LogWarning("⚠️ Viên đạn thiếu script NPCBullet!");
        }
    }
}