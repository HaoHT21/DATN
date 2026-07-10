using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HostageController : MonoBehaviour
{
    [Header("Định danh")]
    public string hostageId = "hostage_01";

    [Header("Hội thoại sau mở lồng")]
    public CycleContent[] rescueDialogue;
    public float dialogueTypingSpeed = 0.05f;
    public float postDialogueDelay = 0f;

    public CycleContent[] RescueDialogue => rescueDialogue;
    public float DialogueTypingSpeed => dialogueTypingSpeed;
    public float PostDialogueDelay => postDialogueDelay;
    public string HostageId => hostageId;

    [Header("Hành vi sau giải cứu")]
    public HostageRescueMode rescueMode = HostageRescueMode.Disappear;

    [Header("Chuyển scene sau giải cứu")]
    public bool transferAfterRescue = true;
    public string targetSceneName = "Sanh";
    public string targetSpawnPointId = "default";

    [Header("Walk To Exit")]
    public Transform exitPoint;
    public float moveSpeed = 2.5f;
    public float stoppingDistance = 0.2f;

    [Header("Follow Player")]
    public float followDistance = 1.5f;

    [Header("Disappear")]
    public float disappearDelay = 0.5f;

    [Header("Phần thưởng gem")]
    public bool giveGemReward = true;
    public ItemData rewardGem;
    public GameObject gemGiftPrefab;
    public AudioClip gemGiftSound;

    public Transform houseSpawnPoint;
    public GameObject hostagePrefab;

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
        TryGiveGemReward();

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

    private void TryGiveGemReward()
    {
        if (!giveGemReward || rewardGem == null)
            return;

        GameObject prefab = gemGiftPrefab != null ? gemGiftPrefab : rewardGem.itemPrefab;
        if (prefab != null)
        {
            GameObject gift = Instantiate(prefab, transform.position, Quaternion.identity);

            GemPickup pickup = gift.GetComponent<GemPickup>();
            if (pickup != null)
                Destroy(pickup);

            HostageGemGift giftBehaviour = gift.GetComponent<HostageGemGift>();
            if (giftBehaviour == null)
                giftBehaviour = gift.AddComponent<HostageGemGift>();

            giftBehaviour.gemData = rewardGem;
            giftBehaviour.pickupSound = gemGiftSound;

            if (gift.GetComponent<ChestLootPop>() == null)
                gift.AddComponent<ChestLootPop>();

            return;
        }

        if (GemInventoryHelper.TryGiveGem(rewardGem))
            PlaySound(gemGiftSound);
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
        RegisterTransferIfNeeded();

        if (hostagePrefab != null && houseSpawnPoint != null)
        {
            Instantiate(
                hostagePrefab,
                houseSpawnPoint.position,
                houseSpawnPoint.rotation);
        }

        Destroy(gameObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }

    private void RegisterTransferIfNeeded()
    {
        if (!transferAfterRescue || string.IsNullOrEmpty(hostageId))
            return;

        HostageRescueManager manager = HostageRescueManager.EnsureInstance();
        manager.RegisterRescue(hostageId, targetSceneName, targetSpawnPointId);
    }
}
