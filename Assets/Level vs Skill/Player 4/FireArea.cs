using UnityEngine;
using System.Collections.Generic;

public class FireArea : MonoBehaviour
{
    public int damagePerSecond = 20; // 20 sát thương mỗi giây theo yêu cầu
    public float skillDuration = 3f;  // Tồn tại trong 3 giây (hoặc 5s tùy mày chỉnh ngoài Inspector)
    public LayerMask enemyLayer;     // Layer của Quái (Enemy)

    // Danh sách lưu các con quái/Boss đang đứng trong vùng lửa và thời gian đốt tiếp theo của tụi nó
    private Dictionary<EnemyHeath, float> burntEnemies = new Dictionary<EnemyHeath, float>();

    void Start()
    {
        // Tự động xóa sổ vùng lửa sau đúng thời gian tồn tại
        Destroy(gameObject, skillDuration);
    }

    void Update()
    {
        // Tạo một mảng danh sách tạm thời để duyệt qua Dictionary mà không bị lỗi bộ nhớ
        List<EnemyHeath> keys = new List<EnemyHeath>(burntEnemies.Keys);

        foreach (EnemyHeath enemyHP in keys)
        {
            if (enemyHP == null || enemyHP.IsDead)
            {
                burntEnemies.Remove(enemyHP);
                continue;
            }

            // Nếu đã đến lúc đốt máu (mỗi 1 giây trôi qua)
            if (Time.time >= burntEnemies[enemyHP])
            {
                enemyHP.TakeDamage(damagePerSecond);
                Debug.Log($"<color=orange>[Hỏa Tuyến]</color> Đang thiêu đốt {enemyHP.name}: -{damagePerSecond} HP!");

                // Cập nhật mốc thời gian cho giây tiếp theo
                burntEnemies[enemyHP] = Time.time + 1f;
            }
        }
    }

    // Khi con quái hoặc Boss bước chân vào vùng lửa
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) return;

        // BẢO VỆ CHÍ MẠNG: Check xem có đụng độ LayerEnemy, hoặc đụng trúng Tag "Boss"/"Enemy", hoặc Layer mang tên "Boss"
        bool isEnemyLayer = ((1 << collision.gameObject.layer) & enemyLayer) != 0;
        bool isBossTag = collision.CompareTag("Boss") || collision.CompareTag("Enemy");
        bool isBossLayer = LayerMask.LayerToName(collision.gameObject.layer) == "Boss";

        if (isEnemyLayer || isBossTag || isBossLayer)
        {
            EnemyHeath enemyHP = collision.GetComponent<EnemyHeath>();
            if (enemyHP != null && !enemyHP.IsDead && !burntEnemies.ContainsKey(enemyHP))
            {
                // Gây sát thương ngay lập tức phát đầu tiên khi vừa chạm lửa
                enemyHP.TakeDamage(damagePerSecond);
                Debug.Log($"<color=red>[Hỏa Tuyến]</color> {enemyHP.name} dẫm phải lửa: -{damagePerSecond} HP!");

                // Đặt lịch cho giây tiếp theo (1 giây sau mới đốt tiếp)
                burntEnemies.Add(enemyHP, Time.time + 1f);
            }
        }
    }

    // Khi con quái hoặc Boss chạy thoát ra khỏi vùng lửa thì ngừng đốt
    private void OnTriggerExit2D(Collider2D collision)
    {
        EnemyHeath enemyHP = collision.GetComponent<EnemyHeath>();
        if (enemyHP != null && burntEnemies.ContainsKey(enemyHP))
        {
            burntEnemies.Remove(enemyHP);
            Debug.Log($"<color=yellow>[Hỏa Tuyến]</color> {collision.name} đã thoát khỏi vùng lửa!");
        }
    }
}