using UnityEngine;

public class PlayerFireSkill : MonoBehaviour
{
    [Header("--- VỊ TRÍ PHUN LỬA CHUẨN XÁC ---")]
    public Transform firePoint;

    [Header("--- CẤU HÌNH SKILL PHUN LỬA (PHÍM I) ---")]
    public GameObject firePrefab;      // Kéo Prefab lửa FireSkillEffect vào đây
    public float cooldown = 7f;
    private float _cooldownTimer = 0f;

    private SpriteRenderer _sprite;
    private GameObject currentFireInstance; // Lưu vết cục lửa đang phun trên Map
    private Transform finalFirePoint;
    private Vector3 _baseFireScale;          // Lưu lại kích thước gốc của cụm lửa

    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        // Nhấn phím I để bắt đầu phun lửa
        if (Input.GetKeyDown(KeyCode.I) && _cooldownTimer <= 0 && firePrefab != null)
        {
            CastFireSkill();
        }

        // ĐOẠN UPDATE SỬA LỖI XOAY MẶT & BÁM ĐUÔI REAL-TIME:
        if (currentFireInstance != null && finalFirePoint != null)
        {
            // 1. TÍNH TOÁN VỊ TRÍ PHUN CHUẨN XÁC KHI PLAYER QUAY MẶT:
            // Vì nhóm mày lật mặt bằng '_sprite.flipX', các điểm con (FirePoint) sẽ không tự đối xứng sang bên kia.
            // Do đó code phải tự tính toán khoảng lệch (Offset) để đưa ngọn lửa ra đúng trước mặt Player.
            Vector3 localOffset = transform.InverseTransformPoint(finalFirePoint.position);

            if (_sprite != null && _sprite.flipX)
            {
                // Nếu Player quay trái, đảo ngược trục X của điểm bắn
                localOffset.x = -Mathf.Abs(localOffset.x);
            }
            else
            {
                // Nếu Player quay phải, giữ nguyên trục X dương
                localOffset.x = Mathf.Abs(localOffset.x);
            }

            // Ép ngọn lửa chạy theo vị trí đã sửa lỗi đối xứng
            currentFireInstance.transform.position = transform.TransformPoint(localOffset);

            // 2. ÉP XOAY MẶT CỤM LỬA THEO PLAYER CHUẨN 100%:
            if (_sprite != null)
            {
                // Dựa vào scale gốc (_baseFireScale) để lật trục X, tránh lỗi co rúm hay nhảy kích thước
                float targetScaleX = _sprite.flipX ? -Mathf.Abs(_baseFireScale.x) : Mathf.Abs(_baseFireScale.x);
                currentFireInstance.transform.localScale = new Vector3(targetScaleX, _baseFireScale.y, _baseFireScale.z);
            }
        }
    }

    void CastFireSkill()
    {
        _cooldownTimer = cooldown;

        // Xác định điểm bắn
        finalFirePoint = firePoint;
        if (finalFirePoint == null)
        {
            finalFirePoint = transform.Find("FP") ?? transform.Find("WeaponHolder/FP") ?? transform.Find("FirePoint") ?? transform.Find("WeaponHolder/FirePoint");
        }

        Vector3 spawnPos = (finalFirePoint != null) ? finalFirePoint.position : transform.position;

        // Đẻ lửa ra độc lập ngoài Map (Không làm con Player để tránh lỗi méo Collider do Scale của Player)
        currentFireInstance = Instantiate(firePrefab, spawnPos, Quaternion.identity);

        // Lưu lại kích thước ban đầu của Prefab lửa ngay khi vừa đẻ ra
        _baseFireScale = currentFireInstance.transform.localScale;

        // Thiết lập hướng quay đầu tiên dựa theo hướng nhìn hiện tại của Player
        if (_sprite != null && _sprite.flipX)
        {
            currentFireInstance.transform.localScale = new Vector3(-Mathf.Abs(_baseFireScale.x), _baseFireScale.y, _baseFireScale.z);
        }
        else
        {
            currentFireInstance.transform.localScale = new Vector3(Mathf.Abs(_baseFireScale.x), _baseFireScale.y, _baseFireScale.z);
        }

        Debug.Log($"<color=orange>[Hỏa Tuyến]</color> Đã kích hoạt lửa bám theo điểm: {(finalFirePoint != null ? finalFirePoint.name : "Center")}");
    }
}