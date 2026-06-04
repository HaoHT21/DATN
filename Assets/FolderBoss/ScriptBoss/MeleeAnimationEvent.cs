using UnityEngine;

public class MeleeAnimationEvent : MonoBehaviour
{
    [Header("Melee")]
    public GameObject attackObject;

    private void Start()
    {
        if (attackObject != null)
            attackObject.SetActive(false);
    }

    // ======================
    // MELEE
    // ======================

    public void EnableAttack()
    {
        if (attackObject != null)
            attackObject.SetActive(true);
    }

    public void DisableAttack()
    {
        if (attackObject != null)
            attackObject.SetActive(false);
    }


}