using UnityEngine;

/// <summary>
/// Gắn lên GameObject Boss (cùng object có Animator).
/// Thêm Animation Event vào clip Attack1/Attack2:
/// - EnableAttack1Hitbox / DisableAttack1Hitbox
/// - EnableAttack2Hitbox / DisableAttack2Hitbox
/// - OnAttackAnimationFinished
/// - OnHurtAnimationFinished
/// </summary>
public class BossEnemyAnimationEvents : MonoBehaviour
{
    [Header("Hitbox")]
    public GameObject attack1Hitbox;
    public GameObject attack2Hitbox;

    private BossEnemyAI _bossAI;

    private void Awake()
    {
        _bossAI = GetComponent<BossEnemyAI>();

        if (attack1Hitbox == null && _bossAI != null)
            attack1Hitbox = _bossAI.attack1Hitbox;

        if (attack2Hitbox == null && _bossAI != null)
            attack2Hitbox = _bossAI.attack2Hitbox;
    }

    public void EnableAttack1Hitbox()
    {
        if (attack1Hitbox != null)
            attack1Hitbox.SetActive(true);
    }

    public void DisableAttack1Hitbox()
    {
        if (attack1Hitbox != null)
            attack1Hitbox.SetActive(false);
    }

    public void EnableAttack2Hitbox()
    {
        if (attack2Hitbox != null)
            attack2Hitbox.SetActive(true);
    }

    public void DisableAttack2Hitbox()
    {
        if (attack2Hitbox != null)
            attack2Hitbox.SetActive(false);
    }

    public void OnAttackAnimationFinished()
    {
        _bossAI?.OnAttackAnimationFinished();
    }

    public void OnHurtAnimationFinished()
    {
        _bossAI?.OnHurtAnimationFinished();
    }
}
