using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HostageController : MonoBehaviour
{
    [Header("Hành vi sau giải cứu")]
    public HostageRescueMode rescueMode = HostageRescueMode.Disappear;

    [Header("Walk To Exit")]
    public Transform exitPoint;
    public float moveSpeed = 2.5f;
    public float stoppingDistance = 0.2f;

    [Header("Follow Player")]
    public float followDistance = 1.5f;

    [Header("Disappear")]
    public float disappearDelay = 0.5f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _sprite;
    private Transform _followTarget;
    private bool _isRescued;
    private bool _isActive;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        SetMovementLocked(true);
    }

    public void OnRescued()
    {
        if (_isRescued)
            return;

        _isRescued = true;
        SetMovementLocked(false);

        switch (rescueMode)
        {
            case HostageRescueMode.Disappear:
                StartDisappear();
                break;

            case HostageRescueMode.WalkToExit:
                StartWalkToExit();
                break;

            case HostageRescueMode.FollowPlayer:
                StartFollowPlayer();
                break;
        }
    }

    private void Update()
    {
        if (!_isRescued || !_isActive)
            return;

        switch (rescueMode)
        {
            case HostageRescueMode.WalkToExit:
                UpdateWalkToExit();
                break;

            case HostageRescueMode.FollowPlayer:
                UpdateFollowPlayer();
                break;
        }
    }

    private void StartDisappear()
    {
        _isActive = false;
        _rb.linearVelocity = Vector2.zero;
        SetWalking(false);
        Invoke(nameof(DestroyHostage), disappearDelay);
    }

    private void StartWalkToExit()
    {
        if (exitPoint == null)
        {
            Debug.LogWarning($"[Hostage] {name} thiếu exitPoint, chuyển sang biến mất.");
            rescueMode = HostageRescueMode.Disappear;
            StartDisappear();
            return;
        }

        _isActive = true;
    }

    private void StartFollowPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning($"[Hostage] {name} không tìm thấy Player, chuyển sang biến mất.");
            rescueMode = HostageRescueMode.Disappear;
            StartDisappear();
            return;
        }

        _followTarget = player.transform;
        _isActive = true;
    }

    private void UpdateWalkToExit()
    {
        float distance = Vector2.Distance(transform.position, exitPoint.position);

        if (distance <= stoppingDistance)
        {
            _rb.linearVelocity = Vector2.zero;
            SetWalking(false);
            _isActive = false;
            DestroyHostage();
            return;
        }

        Vector2 direction = (exitPoint.position - transform.position).normalized;
        _rb.linearVelocity = direction * moveSpeed;
        UpdateFacing(direction.x);
        SetWalking(true);
    }

    private void UpdateFollowPlayer()
    {
        if (_followTarget == null)
        {
            _rb.linearVelocity = Vector2.zero;
            SetWalking(false);
            return;
        }

        float distance = Vector2.Distance(transform.position, _followTarget.position);

        if (distance <= followDistance)
        {
            _rb.linearVelocity = Vector2.zero;
            SetWalking(false);
            return;
        }

        Vector2 direction = (_followTarget.position - transform.position).normalized;
        _rb.linearVelocity = direction * moveSpeed;
        UpdateFacing(direction.x);
        SetWalking(true);
    }

    private void SetMovementLocked(bool locked)
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.simulated = !locked;
        SetWalking(false);
    }

    private void UpdateFacing(float directionX)
    {
        if (_sprite != null && Mathf.Abs(directionX) > 0.01f)
            _sprite.flipX = directionX < 0f;
    }

    private void SetWalking(bool isWalking)
    {
        if (_animator != null)
            _animator.SetBool("isWalking", isWalking);
    }

    private void DestroyHostage()
    {
        Destroy(gameObject);
    }
}
