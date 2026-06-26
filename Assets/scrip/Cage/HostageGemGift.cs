using System.Collections;
using UnityEngine;

/// <summary>
/// Gem hiển thị tạm thời khi con tin tặng thưởng, tự thêm vào túi sau khi bật lên.
/// </summary>
[DisallowMultipleComponent]
public class HostageGemGift : MonoBehaviour
{
    public ItemData gemData;
    public AudioClip pickupSound;
    public float collectDelay = 0.65f;

    private bool _collected;

    private void Start()
    {
        StartCoroutine(CollectRoutine());
    }

    private IEnumerator CollectRoutine()
    {
        yield return new WaitForSeconds(collectDelay);
        Collect();
    }

    private void Collect()
    {
        if (_collected)
            return;

        _collected = true;

        if (gemData != null && GemInventoryHelper.TryGiveGem(gemData))
            PlaySound(pickupSound);

        Destroy(gameObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }
}
