using UnityEngine;

public class PlayerSkillReceiver : MonoBehaviour
{
    public NPCSkill currentSkill;

    private float nextUseTime;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            UseSkill();
        }
    }

    //--------------------------------------------------
    // Nhận Skill từ NPC
    //--------------------------------------------------

    public void SetSkill(NPCSkill skill)
    {
        currentSkill = skill;

        if (UISkillManager.Instance == null)
            return;

        if (skill == null)
            UISkillManager.Instance.HideSkill();
        else
            UISkillManager.Instance.ShowSkill(skill);
    }

    //--------------------------------------------------
    // Sử dụng Skill
    //--------------------------------------------------

    void UseSkill()
    {
        if (currentSkill == null)
            return;

        if (Time.time < nextUseTime)
            return;
        // Tổng thời gian khóa skill
        nextUseTime =
        Time.time +
        currentSkill.duration +
        currentSkill.cooldown;
        // Dùng skill
        currentSkill.Use(gameObject);

        // Cập nhật UI
        if (UISkillManager.Instance != null)
        {
            UISkillManager.Instance.StartDuration(
                currentSkill.duration,
                currentSkill.cooldown
            );
        }
    }
}