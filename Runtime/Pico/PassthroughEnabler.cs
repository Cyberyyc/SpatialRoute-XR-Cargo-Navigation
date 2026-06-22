using UnityEngine;
using Unity.XR.PXR;

/// <summary>
/// Enables PICO video see-through for MR demos.
/// Requires camera clear flags Solid Color with alpha 0.
/// </summary>
public class PassthroughEnabler : MonoBehaviour
{
    [SerializeField] bool enableOnStart = true;

    void Start()
    {
        if (enableOnStart)
            ApplyPassthrough(true);
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause && enableOnStart)
            ApplyPassthrough(true);
    }

    public static void ApplyPassthrough(bool enable)
    {
        foreach (var cam in Camera.allCameras)
        {
            if (cam == null)
                continue;

            cam.clearFlags = CameraClearFlags.SolidColor;
            var bg = cam.backgroundColor;
            bg.a = 0f;
            cam.backgroundColor = bg;
        }

        PXR_Manager.EnableVideoSeeThrough = enable;
    }
}
