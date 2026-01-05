using RoadTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Junction Builder", true)]
public class JunctionBuilderOverlay : Overlay
{
    private Label selectionLabel;

    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement()
        {
            style = {
        width = 200,
        paddingTop = 10,
        paddingBottom = 10,
        paddingLeft = 10,
        paddingRight = 10
    }
        };
        selectionLabel = new Label("Select Knots");
        root.Add(selectionLabel);

        var btn = new Button(OnBuildJunction) { text = "Build Junction" };
        root.Add(btn);

        // Auto-update UI when selection changes
        Selection.selectionChanged += UpdateSelectionInfo;
        return root;
    }

    private void UpdateSelectionInfo()
    {
        var selection = SplineToolUtility.GetSelection();
        selectionLabel.text = "";
        foreach (var element in selection)
        {
            selectionLabel.text += $"Spline {element.targetIndex}, Knot {element.knotIndex}\n";
        }
    }

    private void OnBuildJunction()
    {
        // Get the spline selection
        var selection = SplineToolUtility.GetSelection();
        if (selection.Count < 2) return;

        var roadManager = Selection.activeGameObject?.GetComponent<SplineRoadManager>();
        if (roadManager == null) return;

        InterSection intersection = new InterSection();
        foreach (var item in selection)
        {
            var container = (SplineContainer)item.target;
            var spline = container.Splines[item.targetIndex];
            intersection.AddJunction(item.targetIndex, item.knotIndex, spline, spline[item.knotIndex]);
        }

        Undo.RecordObject(roadManager, "Build Junction");
        roadManager.AddIntersection(intersection);
    }
}