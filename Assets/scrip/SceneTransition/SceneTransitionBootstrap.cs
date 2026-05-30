using UnityEngine;

namespace SceneTransition
{
    /// <summary>
    /// Tùy chọn: đặt prefab SceneTransition vào scene đầu tiên (Main Menu).
    /// Nếu đã có Instance thì không tạo thêm.
    /// </summary>
    public sealed class SceneTransitionBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject sceneTransitionPrefab;

        private void Awake()
        {
            if (SceneTransitionManager.Instance != null || sceneTransitionPrefab == null)
                return;

            Instantiate(sceneTransitionPrefab);
        }
    }
}
