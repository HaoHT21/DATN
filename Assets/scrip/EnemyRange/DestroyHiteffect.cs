using UnityEngine;


public class DestroyHiteffect : MonoBehaviour
{
   [Header("Life Time")]
   public float lifeTime = 2f;
   private void Start()
   {
    Destroy(gameObject, lifeTime);
   }
}
