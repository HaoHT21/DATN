using Fusion;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    public override void Spawned()
    {
        // Chỉ máy của mình mới điều khiển Camera của mình
        if (HasInputAuthority)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.SetParent(transform); // Dính camera vào nhân vật
                cam.transform.localPosition = new Vector3(0, 0, -10); // Khoảng cách nhìn
            }
        }
    }
}