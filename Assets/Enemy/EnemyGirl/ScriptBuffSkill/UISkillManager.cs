using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillManager : MonoBehaviour
{
    public static UISkillManager Instance;

    [Header("UI")]
    public GameObject skillObject;

    public Image imageSkill;

    public Image timeSkill;

    private float duration;
    private bool isDuration;

    private float cooldown;
    private float timer;
    private bool isCooldown;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        HideSkill();
    }

    void Update()
    {
        //--------------------------------
        // Skill đang hoạt động
        //--------------------------------

        if (isDuration)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                isDuration = false;

                StartCooldown(cooldown);
            }

            if (timeSkill != null)
                timeSkill.fillAmount = 1f;

            return;
        }

        //--------------------------------
        // Đang hồi chiêu
        //--------------------------------

        if (!isCooldown)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = 0;
            isCooldown = false;
        }

        if (timeSkill != null)
        {
            timeSkill.fillAmount = timer / cooldown;
        }
    }

    //--------------------------------------------------
    // Hiện Skill
    //--------------------------------------------------


    public void ShowSkill(NPCSkill skill)
    {
        if (skill == null)
        {
            HideSkill();
            return;
        }

        skillObject.SetActive(true);

        imageSkill.sprite = skill.skillIcon;

        timeSkill.fillAmount = 0;
    }

    //--------------------------------------------------
    // Ẩn Skill
    //--------------------------------------------------

    public void HideSkill()
    {
        skillObject.SetActive(false);

        if (timeSkill != null)
            timeSkill.fillAmount = 0;

        isCooldown = false;
    }

    //--------------------------------------------------
    // Bắt đầu hồi chiêu
    //--------------------------------------------------

    public void StartCooldown(float cd)
    {
        cooldown = cd;

        timer = cooldown;

        isCooldown = true;

        if (timeSkill != null)
            timeSkill.fillAmount = 1f;
    }

    public void StartDuration(float dur, float cd)
    {
        duration = dur;
        cooldown = cd;

        timer = duration;

        isDuration = true;
        isCooldown = false;

        if (timeSkill != null)
            timeSkill.fillAmount = 1f;
    }
}