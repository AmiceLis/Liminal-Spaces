using UnityEngine;
using UnityEngine.InputSystem;
using VolumetricFogAndMist2;

// Raises and lowers the density of a Volumetric Fog volume.
//
// Two ways to drive it:
//   1. Call RaiseFog() or LowerFog() every frame while something is happening
//      (for example, while a hand is inside a trigger).
//   2. Call StartRaisingFog(), StartLoweringFog() and StopFog() once, and this
//      script keeps changing the density in Update until told to stop.
public class FogRaiser : MonoBehaviour
{
    [Tooltip("The Volumetric Fog component on the Fog Volume object.")]
    public VolumetricFog sceneFog;

    [Header("Speed (density change per second)")]
    public float raiseSpeed = 0.1f;
    public float lowerSpeed = 0.1f;

    [Header("Limits")]
    public float minDensity = 0f;
    public float maxDensity = 1f;

    [Header("Testing")]
    [Tooltip("Hold Up Arrow to raise and Down Arrow to lower the fog in Play mode.")]
    public bool useKeyboardForTesting = true;

    [Header("Read only (shown for debugging)")]
    public float currentDensity;

    VolumetricFogProfile fogProfile;
    int direction; // 1 = raising, -1 = lowering, 0 = stopped

    void Start()
    {
        if (sceneFog == null)
        {
            Debug.LogWarning("FogRaiser: no Volumetric Fog assigned.", this);
            enabled = false;
            return;
        }

        // "settings" gives this fog its own copy of the profile, so changing the
        // density at runtime does not overwrite the shared profile asset on disk.
        fogProfile = sceneFog.settings;
        currentDensity = fogProfile.density;
    }

    void Update()
    {
        if (useKeyboardForTesting && Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                RaiseFog();
            }
            else if (Keyboard.current.downArrowKey.isPressed)
            {
                LowerFog();
            }
        }

        if (direction > 0)
        {
            RaiseFog();
        }
        else if (direction < 0)
        {
            LowerFog();
        }
    }

    // Increase the fog density by raiseSpeed per second. Call every frame.
    public void RaiseFog()
    {
        ChangeDensity(raiseSpeed * Time.deltaTime);
    }

    // Decrease the fog density by lowerSpeed per second. Call every frame.
    public void LowerFog()
    {
        ChangeDensity(-lowerSpeed * Time.deltaTime);
    }

    // Keep raising the fog every frame until StopFog() is called.
    public void StartRaisingFog()
    {
        direction = 1;
    }

    // Keep lowering the fog every frame until StopFog() is called.
    public void StartLoweringFog()
    {
        direction = -1;
    }

    public void StopFog()
    {
        direction = 0;
    }

    void ChangeDensity(float amount)
    {
        if (fogProfile == null)
        {
            return;
        }

        float newDensity = Mathf.Clamp(fogProfile.density + amount, minDensity, maxDensity);
        if (Mathf.Approximately(newDensity, fogProfile.density))
        {
            return;
        }

        fogProfile.density = newDensity;
        currentDensity = newDensity;
        sceneFog.UpdateMaterialProperties();
    }
}
