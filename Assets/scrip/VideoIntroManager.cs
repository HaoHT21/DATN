using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoIntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "Tên_Scene_Game";

    void Start()
    {
        // ??ng ký s? ki?n khi video ch?y xong
        videoPlayer.loopPointReached += OnVideoFinished;
    }
    void Update()
    {
        // Nh?n phím b?t k? ho?c click chu?t ?? b? qua video
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
    public void PlayVideo()
    {
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}