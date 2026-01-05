using System.Collections.Generic;
using UnityEditor.Splines;
using UnityEngine;

namespace RoadTool
{
    public static class SplineToolUtility
    {
        public static bool HasSelection()
        {
            return SplineSelection.HasActiveSplineSelection();
        }

        public struct SelectedSplineElementInfo
        {
            public Object target;
            public int targetIndex;
            public int knotIndex;

            public SelectedSplineElementInfo(Object @object, int index, int knot)
            {
                target = @object;
                targetIndex = index;
                knotIndex = knot;
            }
        }

        public static List<SelectedSplineElementInfo> GetSelection()
        {
            //get internal struct data
            List<SelectableSplineElement> elements = SplineSelection.selection;

            //Make new public struct data
            List<SelectedSplineElementInfo> infos = new List<SelectedSplineElementInfo>();

            foreach (SelectableSplineElement element in elements)
            {
                infos.Add(new SelectedSplineElementInfo(element.target, element.targetIndex, element.knotIndex));
            }

            return infos;
        }
    }
}
