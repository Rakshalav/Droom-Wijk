using UnityEngine;
using UnityEngine.EventSystems;

// Implement the interfaces required for drag handling
public class MapDragHandler : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    // A reference to the Map Camera we want to move/pan
    public Camera fullMapCamera;
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    void Start()
    {
        // ... other code ...
        if (fullMapCamera != null)
        {
            fullMapCamera.enabled = false;
            Debug.Log("Map Camera should be off.");
        }
        else
        {
            Debug.LogError("ERROR: MapToggle fullMapCamera field is missing in the Inspector!");
        }
        // ... other code ...
    }


    // Called when the pointer is pressed down on the UI element
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        // Capture initial mouse position in screen space
        lastMousePosition = Input.mousePosition;
    }

    // Called while the pointer is being dragged over the UI element
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && fullMapCamera != null)
        {
            // Calculate how much the mouse moved since the last frame
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // Pan the camera in the opposite direction of the mouse movement
            // Scale by Time.deltaTime or a sensitivity value if needed, but this works fundamentally
            fullMapCamera.transform.Translate(-delta.x * Time.deltaTime * 5, -delta.y * Time.deltaTime * 5, 0);

            lastMousePosition = Input.mousePosition;
        }
    }

    // Called when the pointer is released
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
}
