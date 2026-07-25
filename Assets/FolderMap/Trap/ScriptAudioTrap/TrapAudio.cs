using UnityEngine;

[RequireComponent(typeof(TrapController))]
public class TrapAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BẪY GAI ---")]
    [Tooltip("Kéo file âm thanh gai đâm/kiếm đâm (stab) vào đây")]
    public AudioClip spikeStabSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng từ 0 đến 100

    [Header("--- BỘ LỌC KHOẢNG CÁCH GẦN PLAYER ---")]
    [Tooltip("Kéo thả GameObject đại diện Player vào đây")]
    public Transform playerTransform;

    [Tooltip("Khoảng cách (bán kính) tối đa từ Player tới gai để có thể nghe thấy âm thanh")]
    public float hearingRadius = 7f;

    private TrapController _trapController;
    private Collider2D _trapCollider;
    private bool _lastColliderState = false;

    private void Awake()
    {
        _trapController = GetComponent<TrapController>();
    }

    private void Start()
    {
        if (_trapController != null)
        {
            _trapCollider = _trapController.trapCollider;
            if (_trapCollider != null)
            {
                _lastColliderState = _trapCollider.enabled;
            }

            DisableOriginalTrapSound();
        }

        FindPlayerTarget();
    }

    // Hàm riêng chuyên tìm Player trên Map
    public void FindPlayerTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (_trapCollider == null || _trapController == null) return;

        // 🎯 CỰC QUAN TRỌNG: Nếu Player cũ bị Destroy (null), tự tìm ngay con Player mới!
        if (playerTransform == null)
        {
            FindPlayerTarget();
            if (playerTransform == null) return; // Nếu vẫn không tìm thấy thì mới bỏ qua
        }

        // Theo dõi sự thay đổi trạng thái bật/tắt của Collider từ TrapController
        bool currentColliderState = _trapCollider.enabled;

        // Nếu lúc trước bẫy đang tắt (false) mà bây giờ bật lên (true) -> Gai trồi lên đâm!
        if (currentColliderState && !_lastColliderState)
        {
            // Kiểm tra khoảng cách thực tế giữa Player mới và vùng bẫy gai
            Vector3 closestPointOnTrap = _trapCollider.ClosestPoint(playerTransform.position);
            float distanceToTrap = Vector3.Distance(playerTransform.position, closestPointOnTrap);

            // Nếu Player đang đứng đủ gần trong tầm nghe thì mới phát âm thanh
            if (distanceToTrap <= hearingRadius)
            {
                PlaySpikeSound(closestPointOnTrap);
            }
        }

        // Cập nhật lại trạng thái cũ cho khung hình tiếp theo
        _lastColliderState = currentColliderState;
    }

    private void PlaySpikeSound(Vector3 spawnPosition)
    {
        if (spikeStabSound != null)
        {
            GameObject tempAudio = new GameObject("TempTrapSpikeAudio");
            tempAudio.transform.position = spawnPosition;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = spikeStabSound;
            aSource.spatialBlend = 0f;
            aSource.volume = Mathf.Clamp01(soundVolume / 100f);

            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.envGroup;
            }

            aSource.Play();
            Destroy(tempAudio, spikeStabSound.length);
        }
    }

    private void DisableOriginalTrapSound()
    {
        if (_trapController == null) return;

        System.Type type = _trapController.GetType();
        string[] soundFields = { "sound_gai", "soundGai", "spikeSound", "trapSound" };

        foreach (string fieldName in soundFields)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(_trapController, null);
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, hearingRadius);
        }
    }
}