using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [Header("Minimap")]
    public Camera minimapCam;
    public RawImage minimapUI;

    [Header("Full Map")]
    public Camera fullMapCam;
    public RawImage fullMapUI;

    [Header("Controls")]
    public KeyCode fullMapKey = KeyCode.L;
    public KeyCode minimapToggleKey = KeyCode.M;

    private bool isFullMapOpen = false;
    private bool minimapPreferredState = true;

    void Start()
    {
        // Initial state
        UpdateVisuals();
    }

    void Update()
    {
        if (Input.GetKeyDown(minimapToggleKey))
        {
            minimapPreferredState = !minimapPreferredState;
            UpdateVisuals();
        }

        if (Input.GetKeyDown(fullMapKey))
        {
            isFullMapOpen = !isFullMapOpen;
            UpdateVisuals();
        }
    }

    void UpdateVisuals()
    {
        if (isFullMapOpen)
        {
            // Turn Large Map ON
            ToggleGroup(fullMapCam, fullMapUI, true);
            // Turn Minimap OFF
            ToggleGroup(minimapCam, minimapUI, false);
        }
        else
        {
            // Turn Large Map OFF
            ToggleGroup(fullMapCam, fullMapUI, false);
            // Restore Minimap to preference
            ToggleGroup(minimapCam, minimapUI, minimapPreferredState);
        }
    }

    // This uses SetActive on the GameObjects to ensure they truly vanish
    void ToggleGroup(Camera cam, RawImage ui, bool state)
    {
        if (cam != null) cam.gameObject.SetActive(state);
        if (ui != null) ui.gameObject.SetActive(state);
    }
}