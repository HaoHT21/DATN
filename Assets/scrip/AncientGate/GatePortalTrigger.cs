using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GatePortalTrigger : MonoBehaviour
{
    public AncientGateController gate;

    private void Awake()
    {
        if (gate == null)
            gate = GetComponentInParent<AncientGateController>();

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gate == null || !other.CompareTag("Player"))
            return;

        if (gate.State != GateState.Open)
            return;
    }
}
