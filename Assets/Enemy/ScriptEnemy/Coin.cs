using UnityEngine;

public class Coin : MonoBehaviour
{
    private bool autoDestroy = false;
    private float destroyTime = 10f;

    public void SetAutoDestroy(bool value, float time = 10f)
    {
        autoDestroy = value;
        destroyTime = time;

        if (autoDestroy)
            Destroy(gameObject, destroyTime);
    }
}