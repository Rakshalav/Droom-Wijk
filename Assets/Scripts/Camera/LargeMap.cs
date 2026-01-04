using UnityEngine;
using UnityEngine.UI;

public class Largemap : MonoBehaviour
{
    [Header("Largemap References")]
    public Camera largeMapCamera;
    public RawImage largeMapUI;
    public RawImage minimapUI; // Assuming this should always be visible unless the large map is up
    private bool isLargemapVisible = false;

    private void Start()
    {
        // Set initial state (minimap visible, large map hidden)
        SetLargeMapVisibility(isLargemapVisible);
    }

    private void Update()
    {
        // This input check only runs once when 'M' is pressed down
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Flip the boolean state
            isLargemapVisible = !isLargemapVisible;

            // Apply the new state
            SetLargeMapVisibility(isLargemapVisible);
        }

        // Remove the 'else' block from Update()
        // The visibility should not change every single frame unless M is pressed.
    }

    private void SetLargeMapVisibility(bool isVisible)
    {
        if (largeMapCamera != null)
        {
            largeMapCamera.enabled = isVisible;
        }

        if (largeMapUI != null)
        {
            largeMapUI.gameObject.SetActive(isVisible);
        }

        if (minimapUI != null)
        {
            // Set the minimap to be the opposite of the large map's visibility
            minimapUI.gameObject.SetActive(!isVisible);
        }
    }
}
