using UnityEngine;

public partial class MapController : MonoBehaviour
{
    [Header("Settings")]
    public GameObject mapCamera; // Assign your Orthographic Camera here
    public KeyCode mapKey = KeyCode.L;

    private bool isMapOpen = false;

    void Start()
    {
        // Ensure the map is closed when the game starts
        if (mapCamera != null)
            mapCamera.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(mapKey))
        {
            ToggleMap();
        }
    }

    void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        mapCamera.SetActive(isMapOpen);

        // Optional: Pause time when map is open
        // Time.timeScale = isMapOpen ? 0f : 1f;
    }
}