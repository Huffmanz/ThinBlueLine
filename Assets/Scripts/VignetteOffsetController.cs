using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class VignetteOffsetController : MonoBehaviour
{
    private Camera mainCamera;
    private Volume volume;
    private Vignette vignette;
    private Vector2 originalCenter;
    public float offsetMultiplier = 0.05f; // Adjust the multiplier to control the offset strength

    void Start()
    {
        mainCamera = Camera.main;
        volume = GetComponent<Volume>();
        if (volume.profile.TryGet<Vignette>(out vignette))
        {
            originalCenter = vignette.center.value;
        }
    }

    void Update()
    {
        if (vignette == null)
            return;

        // Get the camera's rotation in world space
        Vector3 cameraRotation = mainCamera.transform.eulerAngles;

        // Normalize the rotation values to range [-1, 1]
        float normalizedX = Mathf.Sin(cameraRotation.y * Mathf.Deg2Rad);
        float normalizedY = Mathf.Sin(cameraRotation.x * Mathf.Deg2Rad);

        // Calculate the offset based on the normalized rotation and multiplier
        Vector2 vignetteOffset = new Vector2(normalizedX * offsetMultiplier, normalizedY * offsetMultiplier);

        // Apply the offset to the vignette
        vignette.center.value = vignetteOffset + originalCenter;
    }
}
