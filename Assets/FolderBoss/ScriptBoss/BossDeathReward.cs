using UnityEngine;

public class BossDeathReward : MonoBehaviour
{
    private GameObject targetObject;

    private void Awake()
    {
        GameObject[] allObjects =
            Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            Debug.Log("Đang kiểm tra: " + obj.name);

            if (obj.CompareTag("Portal"))
            {
                targetObject = obj;

                Debug.Log("Đã tìm thấy Portal: "
                          + obj.name);

                break;
            }
        }

        if (targetObject == null)
        {
            Debug.LogError("Không tìm thấy object tag Portal");
        }
    }

    private void Start()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            Debug.Log("Đã ẩn Portal");
        }
    }

    public void OnBossDeath()
    {
        Debug.Log("OnBossDeath được gọi");

        if (targetObject == null)
        {
            Debug.LogError("Portal NULL");
            return;
        }

        Debug.Log("Hiện Portal: " + targetObject.name);

        targetObject.SetActive(true);
    }
}