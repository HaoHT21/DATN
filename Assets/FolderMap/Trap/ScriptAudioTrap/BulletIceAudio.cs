using UnityEngine;

public class BulletIceAudio : MonoBehaviour
{
    [HideInInspector] public AudioClip hitSound;
    [HideInInspector] public float soundVolume = 1f;

    private bool _hasPlayedHit = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi đạn va chạm với Player hoặc Enemy thì nổ tiếng găm trúng người
        if (_hasPlayedHit) return;

        if (collision.CompareTag("Player") || collision.CompareTag("Enemy") || collision.CompareTag("Boss") || LayerMask.LayerToName(collision.gameObject.layer) == "Boss")
        {
            _hasPlayedHit = true;

            if (hitSound != null)
            {
                GameObject tempAudio = new GameObject("TempIceBulletHitAudio");
                tempAudio.transform.position = transform.position;
                AudioSource aSource = tempAudio.AddComponent<AudioSource>();

                aSource.clip = hitSound;
                aSource.spatialBlend = 0f; // Khóa 2D to rõ
                aSource.volume = soundVolume;

                aSource.Play();
                Destroy(tempAudio, hitSound.length);
            }
        }
    }
}