using System.Collections;
using UnityEngine;

/// <summary>
/// Gắn vào prefab vật phẩm rơi ra từ rương.
/// Vật phẩm bật nhẹ lên rồi rơi xuống; chỉ cho nhặt sau khi chạm đất.
/// </summary>
[DisallowMultipleComponent]
public class ChestLootPop : MonoBehaviour
{
    [Header("Arc (không dùng Rigidbody2D)")]
    public float popHeight = 0.6f;
    public float popUpDuration = 0.25f;
    public float fallDuration = 0.35f;

    [Header("Physics (nếu có Rigidbody2D)")]
    public Vector2 popForce = new Vector2(0f, 4f);
    public float gravityScale = 2f;
    public float settleVelocityThreshold = 0.15f;
    public float maxSettleWait = 2f;

    private Collider2D[] _colliders;
    private Rigidbody2D _rigidbody;
    private bool _pickupEnabled;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        SetPickupEnabled(false);
    }

    private void Start()
    {
        if (_rigidbody != null)
            StartCoroutine(PhysicsPopRoutine());
        else
            StartCoroutine(ArcPopRoutine());
    }

    private IEnumerator ArcPopRoutine()
    {
        Vector3 groundPos = transform.position;
        float elapsed = 0f;

        while (elapsed < popUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popUpDuration);
            float y = Mathf.Lerp(0f, popHeight, t);
            transform.position = groundPos + Vector3.up * y;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);
            float y = Mathf.Lerp(popHeight, 0f, t);
            transform.position = groundPos + Vector3.up * y;
            yield return null;
        }

        transform.position = groundPos;
        SetPickupEnabled(true);
    }

    private IEnumerator PhysicsPopRoutine()
    {
        _rigidbody.gravityScale = gravityScale;
        _rigidbody.linearVelocity = popForce;

        float waited = 0f;
        while (waited < maxSettleWait)
        {
            if (_rigidbody.linearVelocity.sqrMagnitude <= settleVelocityThreshold * settleVelocityThreshold)
                break;

            waited += Time.deltaTime;
            yield return null;
        }

        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.gravityScale = 0f;
        SetPickupEnabled(true);
    }

    private void SetPickupEnabled(bool enabled)
    {
        if (_pickupEnabled == enabled)
            return;

        _pickupEnabled = enabled;

        if (_colliders == null)
            return;

        foreach (Collider2D col in _colliders)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }
}
