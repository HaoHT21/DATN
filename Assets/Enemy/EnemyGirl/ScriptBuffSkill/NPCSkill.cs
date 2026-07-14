using UnityEngine;

public abstract class NPCSkill : MonoBehaviour
{
    public string skillName;

    public Sprite skillIcon;

    [Header("Duration")]
    public float duration = 5f;

    [Header("Cooldown")]
    public float cooldown = 5f;

    public abstract void Use(GameObject player);
}