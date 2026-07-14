using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraNausea : MonoBehaviour
{
    public PostProcessVolume volume;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;

    public float speed = 5f;
    public float intensity = 1f;

    void Start()
    {
        volume.profile.TryGetSettings(out chromaticAberration);
        volume.profile.TryGetSettings(out lensDistortion);
    }

    void Update()
    {
        float timeFactor = Mathf.Sin(Time.time * speed);

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = (timeFactor + 1f) / 2f * intensity;

        if (lensDistortion != null)
            lensDistortion.intensity.value = timeFactor * 20f * intensity;
    }
}