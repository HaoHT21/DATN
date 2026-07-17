using UnityEngine;
using UnityEngine.Playables; // Thư viện cho Timeline

public class CutsceneController : MonoBehaviour
{
    [Header("Cấu hình")]
    public PlayableDirector timeline; // Kéo Timeline của bạn vào đây
    public GameObject winPanel;      // Kéo Win Panel vào đây

    private void Start()
    {
        // Đăng ký sự kiện khi Timeline kết thúc
        if (timeline != null)
        {
            timeline.stopped += OnTimelineStopped;
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        // Timeline đã chạy xong, hiện Panel Win
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
}