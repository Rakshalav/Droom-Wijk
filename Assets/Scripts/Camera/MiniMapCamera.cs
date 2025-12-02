using UnityEngine;
using UnityEngine.UI; // Required for accessing the Image components

public class MiniMap_Camera : MonoBehaviour
{
    // === LINK THESE IN THE INSPECTOR ===
    [Header("Minimap References")]
    public Camera minimapCamera;
    public RawImage minimapUI;
    public Image playerDotUI;
    private bool isMinimapVisible = true;

    void Start()
    {
        SetMinimapVisibility(isMinimapVisible);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isMinimapVisible = !isMinimapVisible;
            SetMinimapVisibility(isMinimapVisible);
        }
    }

    private void SetMinimapVisibility(bool isVisible)
    {
        if (minimapCamera != null)
        {
            minimapCamera.enabled = isVisible;
        }

        if (minimapUI != null)
        {
            minimapUI.gameObject.SetActive(isVisible);
        }

        if (playerDotUI != null)
        {
            playerDotUI.gameObject.SetActive(isVisible);
        }
    }
}
