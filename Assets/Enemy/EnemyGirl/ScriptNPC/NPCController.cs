using System.Net.NetworkInformation;
using UnityEngine;

public enum NPCState
{
    Patrol,
    Follow,
    ReturnHome
}

public class NPCController : MonoBehaviour
{
    [Header("Visual")]
    public Transform visualRoot;

    [Header("Spawn")]
    public Transform homePoint;

    [HideInInspector]
    public NPCState currentState = NPCState.Patrol;

    public NPCSkill npcSkill;

    [HideInInspector]
    public Transform player;

    private void Awake()
    {
        if (homePoint == null)
        {
            GameObject spawn = GameObject.FindWithTag("Spawn");

            if (spawn != null)
                homePoint = spawn.transform;
        }
    }

    public void RotateCharacter(float moveX)
    {
        if (moveX > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (moveX < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        if (visualRoot != null)
        {
            visualRoot.rotation = Quaternion.identity;
        }
    }

    public void StartFollow(Transform target)
    {
        player = target;
        currentState = NPCState.Follow;

        Debug.Log(name + " bắt đầu Follow");
    }

    public void StopFollow()
    {
        player = null;
        currentState = NPCState.ReturnHome;

        Debug.Log(name + " quay về Home");
    }

    public void ArriveHome()
    {
        currentState = NPCState.Patrol;
    }

    public bool IsPatrol()
    {
        return currentState == NPCState.Patrol;
    }

    public bool IsFollow()
    {
        return currentState == NPCState.Follow;
    }

    public bool IsReturnHome()
    {
        return currentState == NPCState.ReturnHome;
    }

    public void Interact()
    {
        Debug.Log(name + " đư?c ngư?i chơi tương tác.");
    }


}