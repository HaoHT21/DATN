using SceneTransition;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Ch? dùng khi KHÔNG có MenuManager ?i?u ph?i intro.
/// Luôn chuy?n c?nh qua SceneTransitionManager (fade tr??c khi load).
/// </summary>
[DisallowMultipleComponent]
public class VideoIntroManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "Sanh";
    [SerializeField] private SceneTransitionMode transitionMode = SceneTransitionMode.Asynchronous;

    private bool _hasStartedTransition;

    private void Reset()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void Update()
    {
        if (_hasStartedTransition || videoPlayer == null || !videoPlayer.isPlaying)
            return;

        if (Input.anyKeyDown)
            BeginTransition();
    }

    public void PlayVideo()
    {
        if (videoPlayer == null)
            return;

        _hasStartedTransition = false;
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        BeginTransition();
    }

    private void BeginTransition()
    {
        if (_hasStartedTransition)
            return;

        _hasStartedTransition = true;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.gameObject.SetActive(false);
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(
                new SceneTransitionRequest(nextSceneName, transitionMode));
            return;
        }

        Debug.LogWarning("[VideoIntroManager] Thi?u SceneTransitionManager — fallback LoadScene.");
        SceneManager.LoadScene(nextSceneName);
    }
}
