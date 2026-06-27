using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("--- 1. SKILL TUNG TÓE MỚI (PHÍM I) ---")]
    public GameObject splatterSkillPrefab;    // Kéo Prefab burst_splatter_003 vào đây
    public float splatterCooldown = 1.5f;
    private float _splatterTimer = 0f;

    [Header("--- 2. SKILL ĐỘC RỈ MÁU (PHÍM L) ---")]
    public GameObject poisonProjectilePrefab; // Kéo Prefab đạn độc vào đây
    public float poisonCooldown = 3f;
    private float _poisonTimer = 0f;

    [Header("--- 3. SKILL NĂNG LƯỢNG 70 DAME (PHÍM M) ---")]
    public GameObject energySkillPrefab;      // Kéo Prefab vòng xoáy sci-fi vào đây
    public float energyCooldown = 2f;
    private float _energyTimer = 0f;

    private SpriteRenderer _sprite;
    private PlayerHealth _playerHealth; // Cầu nối để check Level và trạng thái sống chết gốc

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        // Tự động tìm component PlayerHealth gắn chung trên người con Player 3
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        // TỐI ƯU CHÍ MẠNG: Nếu Player 3 đã nghẻo thì ngắt luôn, đéo cho đếm hồi chiêu hay bấm nút gì hết!
        if (_playerHealth != null && _playerHealth.IsDead) return;

        // Đếm ngược hồi chiêu của cả 3 nút
        if (_splatterTimer > 0) _splatterTimer -= Time.deltaTime;
        if (_poisonTimer > 0) _poisonTimer -= Time.deltaTime;
        if (_energyTimer > 0) _energyTimer -= Time.deltaTime;

        // =================================================================
        // NÚT I: Tung chiêu mới (Splatter) -> MỞ KHÓA TỪ ĐẦU (LEVEL 1)
        // =================================================================
        if (Input.GetKeyDown(KeyCode.I) && _splatterTimer <= 0 && splatterSkillPrefab != null)
        {
            CastSplatterSkill();
        }

        // =================================================================
        // NÚT L: Chiêu Độc cũ -> KHÓA ĐẾN LEVEL 4 MỚI MỞ
        // =================================================================
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (_playerHealth != null && _playerHealth.currentLevel < 4)
            {
                // CHÈN DÒNG NÀY ĐỂ HIỆN CHỮ ĐỎ THÔNG BÁO CHO CHIÊU L:
                if (SkillNotification.Instance != null)
                {
                    SkillNotification.Instance.ShowMessage("CHIÊU [L] ĐANG KHÓA! CẦN LEVEL 4", Color.red);
                }

                Debug.LogWarning($"<color=green>[Skill L đang khóa]</color> Bạn cần đạt Level 4 để mở khóa chiêu Độc Rỉ Máu! (Cấp hiện tại: {_playerHealth.currentLevel})");
                return; // Chặn đứng hoàn toàn
            }

            if (_poisonTimer <= 0 && poisonProjectilePrefab != null)
            {
                CastPoisonSkill();
            }
        }

        // =================================================================
        // NÚT M: Chiêu Năng lượng 70 Dame -> KHÓA ĐẾN LEVEL 7 MỚI MỞ
        // =================================================================
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (_playerHealth != null && _playerHealth.currentLevel < 7)
            {
                // CHÈN DÒNG NÀY ĐỂ HIỆN CHỮ ĐỎ THÔNG BÁO CHO CHIÊU M:
                if (SkillNotification.Instance != null)
                {
                    SkillNotification.Instance.ShowMessage("TUYỆT CHIÊU [M] ĐANG KHÓA! CẦN LEVEL 7", Color.red);
                }

                Debug.LogWarning($"<color=cyan>[Skill M đang khóa]</color> Tuyệt chiêu cuối Năng Lượng cần đạt Level 7 để mở khóa! (Cấp hiện tại: {_playerHealth.currentLevel})");
                return; // Chặn đứng hoàn toàn
            }

            if (_energyTimer <= 0 && energySkillPrefab != null)
            {
                CastEnergySkill();
            }
        }
    }

    private void CastSplatterSkill()
    {
        _splatterTimer = splatterCooldown;
        Vector3 spawnPos = GetSpawnPosition();
        Vector2 dir = (_sprite != null && _sprite.flipX) ? Vector2.left : Vector2.right;

        GameObject proj = Instantiate(splatterSkillPrefab, spawnPos, Quaternion.identity);
        if (proj.TryGetComponent<SplatterBlast>(out var script)) script.moveDirection = dir;
        Debug.Log("<color=yellow>[Manager]</color> Tung chiêu Splatter (Phím I)!");
    }

    private void CastPoisonSkill()
    {
        _poisonTimer = poisonCooldown;
        Vector3 spawnPos = GetSpawnPosition();
        Vector2 dir = (_sprite != null && _sprite.flipX) ? Vector2.left : Vector2.right;

        GameObject proj = Instantiate(poisonProjectilePrefab, spawnPos, Quaternion.identity);
        if (proj.TryGetComponent<PoisonAreaPlayer>(out var script)) script.moveDirection = dir;
        Debug.Log("<color=green>[Manager]</color> Tung chiêu Độc (Phím L)!");
    }

    private void CastEnergySkill()
    {
        _energyTimer = energyCooldown;
        Vector3 spawnPos = GetSpawnPosition();
        Vector2 dir = (_sprite != null && _sprite.flipX) ? Vector2.left : Vector2.right;

        GameObject proj = Instantiate(energySkillPrefab, spawnPos, Quaternion.identity);
        if (proj.TryGetComponent<EnergyBlast>(out var script)) script.moveDirection = dir;
        Debug.Log("<color=cyan>[Manager]</color> Tung chiêu Năng Lượng (Phím M)!");
    }

    private Vector3 GetSpawnPosition()
    {
        Transform myFirePoint = transform.Find("FP") ?? transform.Find("WeaponHolder/FP") ?? transform.Find("FirePoint") ?? transform.Find("WeaponHolder/FirePoint");
        return (myFirePoint != null) ? myFirePoint.position : transform.position;
    }
}