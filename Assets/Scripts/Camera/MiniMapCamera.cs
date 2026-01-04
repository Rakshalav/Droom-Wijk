using UnityEngine;
using UnityEngine.UI;

public class MiniMap_Camera : MonoBehaviour
{
    [Header("Minimap References")]
    public Camera minimapCamera;
    public RawImage minimapUI;
    private bool isMinimapVisible = true;

    void Start()
    {
        SetMinimapVisibility(isMinimapVisible);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
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
    }
}