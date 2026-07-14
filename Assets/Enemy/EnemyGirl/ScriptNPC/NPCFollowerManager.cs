using UnityEngine;

public class NPCFollowerManager : MonoBehaviour
{
    public static NPCFollowerManager Instance;

    [HideInInspector]
    public NPCController currentFollower;

    private void Awake()
    {
        Instance = this;
    }

    public void SelectNPC(NPCController npc)
    {
        PlayerSkillReceiver skill = GetComponent<PlayerSkillReceiver>();

        // Nếu đang chọn chính NPC này thì hủy Follow
        if (currentFollower == npc)
        {
            npc.StopFollow();
            currentFollower = null;

            if (skill != null)
                skill.SetSkill(null);

            Debug.Log("Hủy Follow: " + npc.name);
            return;
        }

        // Nếu đang có NPC khác theo
        if (currentFollower != null)
        {
            currentFollower.StopFollow();
        }

        currentFollower = npc;

        npc.StartFollow(transform);

        if (skill != null)
        {
            skill.SetSkill(npc.npcSkill);
        }

        Debug.Log("NPC Follow: " + npc.name);
    }
}