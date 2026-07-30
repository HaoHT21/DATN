using UnityEngine;
using UnityEngine.Tilemaps;

public class TrapController : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap trapTilemap;           // Tilemap chứa bẫy gai
    public TilemapCollider2D trapCollider; // TilemapCollider2D trực tiếp bật/tắt
    public RuleTile spikeRuleTile;        // Asset Rule Tile bẫy gai

    [Header("Rule Tile Animation Settings")]
    [Tooltip("Tổng số Sprite/Frame trong Animation (Size = 5)")]
    public int totalFrames = 5;

    [Tooltip("Min/Max Speed cấu hình trong Rule Tile (Speed = 1)")]
    public float animationSpeed = 1f;

    [Tooltip("Frame bắt đầu nhô gai lên cao nhất (Frame 5)")]
    public int activeFrame = 5;

    [Header("Damage Settings")]
    public int damage = 10;

    [Tooltip("Thời gian nghỉ giữa các lần trừ máu để tránh 'bay màu' quá nhanh trong 1 đợt gai")]
    public float damageCooldown = 0.5f;

    private float lastDamageTime;

    private void Start()
    {
        if (trapTilemap == null) trapTilemap = GetComponent<Tilemap>();
        if (trapCollider == null) trapCollider = GetComponent<TilemapCollider2D>();

        if (trapCollider != null)
        {
            trapCollider.isTrigger = true;
            trapCollider.enabled = false;
        }
    }

    private void Update()
    {
        // Kiểm tra và cập nhật trạng thái Collider trực tiếp theo thời gian thực mỗi Frame
        UpdateColliderState();
    }

    private void UpdateColliderState()
    {
        if (trapCollider == null) return;

        // 1. Tính tổng thời lượng của 1 chu kỳ (1 cycle)
        float cycleDuration = totalFrames / animationSpeed;

        // 2. Lấy thời gian đã trôi qua trong chu kỳ hiện tại (luôn chuẩn xác theo Time.time)
        float timeInCurrentCycle = Time.time % cycleDuration;

        // 3. Tính chính xác Frame hiện tại (từ 1 đến totalFrames)
        int currentFrame = Mathf.FloorToInt((timeInCurrentCycle / cycleDuration) * totalFrames) + 1;

        // 4. Bật Collider nếu đang ở Frame 5, tắt nếu ở các Frame khác
        bool shouldBeActive = (currentFrame == activeFrame);

        if (trapCollider.enabled != shouldBeActive)
        {
            trapCollider.enabled = shouldBeActive;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Giới hạn tần suất gây sát thương (Cooldown)
        if (Time.time < lastDamageTime + damageCooldown) return;

        Vector3Int cellPosition = trapTilemap.WorldToCell(other.transform.position);
        TileBase currentTile = trapTilemap.GetTile(cellPosition);

        if (currentTile == spikeRuleTile)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                lastDamageTime = Time.time;
                Debug.Log($"[Trap] Gây sát thương tại ô {cellPosition} - Frame {activeFrame}!");
            }
        }
    }
}