using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class SplineRoadManager : MonoBehaviour
{
    [Header("Road Settings")]
    public float m_width = 2f;
    public float resolution = 0.5f;

    [Header("Pole Settings")]
    public GameObject m_polePrefab;
    public float m_poleInterval = 10f;
    public bool m_placeOnBothSides = true;

    public List<InterSection> intersections = new List<InterSection>();
    private MeshFilter meshFilter;

    private bool m_needsPoles = false;

    private void OnEnable() => Spline.Changed += OnSplineChanged;
    private void OnDisable() => Spline.Changed -= OnSplineChanged;
    private void OnSplineChanged(Spline s, int i, SplineModification m) => Rebuild();
    private void OnValidate() => Rebuild();

    private void Update()
    {
        if (m_needsPoles && !Application.isPlaying)
        {
            var splineContainer = GetComponent<SplineContainer>();
            if (splineContainer != null) PlacePoles(splineContainer);
            m_needsPoles = false;
        }
    }

    public void AddIntersection(InterSection newIntersection)
    {
        intersections.Add(newIntersection);
        Rebuild();
    }

    public void Rebuild()
    {
        if (Application.isPlaying) return;

        var splineContainer = GetComponent<SplineContainer>();
        meshFilter = GetComponent<MeshFilter>();
        if (splineContainer == null || meshFilter == null) return;

        BuildRoadAndJunctions(splineContainer);
        m_needsPoles = true;
    }

    private void PlacePoles(SplineContainer container)
    {
        ClearOldPoles();
        if (m_polePrefab == null || m_poleInterval <= 0.1f) return;

        GameObject poleContainer = new GameObject("Poles_Container");
        poleContainer.transform.SetParent(transform, false);

        foreach (var spline in container.Splines)
        {
            float length = spline.GetLength();
            int poleCount = Mathf.FloorToInt(length / m_poleInterval);

            for (int i = 0; i <= poleCount; i++)
            {
                float t = (i * m_poleInterval) / length;
                SampleSplineWidth(spline, t, out Vector3 p1, out Vector3 p2);

                SpawnObject(m_polePrefab, p1, spline, t, poleContainer.transform);
                if (m_placeOnBothSides) SpawnObject(m_polePrefab, p2, spline, t, poleContainer.transform);
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    private void SpawnObject(GameObject prefab, Vector3 position, Spline spline, float t, Transform parent)
    {
        GameObject obj;
#if UNITY_EDITOR
        obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
#else
        obj = Instantiate(prefab, parent);
#endif

        obj.transform.localPosition = position;
        spline.Evaluate(t, out float3 pos, out float3 tangent, out float3 up);
        obj.transform.localRotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)up);
    }

    private void ClearOldPoles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name == "Poles_Container")
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void BuildRoadAndJunctions(SplineContainer container)
    {
        Mesh mesh = new Mesh { name = "CombinedRoadMesh" };
        List<Vector3> allVerts = new List<Vector3>();
        List<int> allTris = new List<int>();

        foreach (var spline in container.Splines)
        {
            float length = spline.GetLength();
            int segments = Mathf.Max(2, Mathf.FloorToInt(length * resolution));
            int vertexStart = allVerts.Count;
            for (int i = 0; i <= segments; i++)
            {
                SampleSplineWidth(spline, (float)i / segments, out Vector3 p1, out Vector3 p2);
                allVerts.Add(p1); allVerts.Add(p2);
                if (i < segments)
                {
                    int root = vertexStart + (i * 2);
                    allTris.Add(root); allTris.Add(root + 2); allTris.Add(root + 1);
                    allTris.Add(root + 1); allTris.Add(root + 2); allTris.Add(root + 3);
                }
            }
        }
        BuildIntersections(allVerts, allTris);
        mesh.SetVertices(allVerts);
        mesh.SetTriangles(allTris, 0);
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
    }

    public void SampleSplineWidth(Spline spline, float t, out Vector3 p1, out Vector3 p2)
    {
        spline.Evaluate(t, out float3 pos, out float3 tangent, out float3 up);
        float3 forward = math.normalize(tangent);
        if (math.all(forward == float3.zero)) forward = new float3(0, 0, 1);
        float3 right = math.normalize(math.cross(up, forward));
        p1 = (Vector3)(pos - (right * m_width));
        p2 = (Vector3)(pos + (right * m_width));
    }

    private void BuildIntersections(List<Vector3> verts, List<int> tris)
    {
        foreach (InterSection intersection in intersections)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 center = Vector3.zero;
            foreach (JunctionInfo junction in intersection.GetJunctions())
            {
                if (junction.spline == null) continue;
                SampleSplineWidth(junction.spline, junction.knotIndex == 0 ? 0 : 1, out Vector3 p1, out Vector3 p2);
                points.Add(p1); points.Add(p2); center += p1 + p2;
            }
            if (points.Count == 0) continue;
            center /= points.Count;
            points.Sort((x, y) => Vector3.SignedAngle(Vector3.forward, x - center, Vector3.up).CompareTo(Vector3.SignedAngle(Vector3.forward, y - center, Vector3.up)));
            int offset = verts.Count;
            for (int j = 1; j <= points.Count; j++)
            {
                verts.Add(center); verts.Add(points[j - 1]);
                verts.Add(j == points.Count ? points[0] : points[j]);
                tris.Add(offset + ((j - 1) * 3)); tris.Add(offset + ((j - 1) * 3) + 1); tris.Add(offset + ((j - 1) * 3) + 2);
            }
        }
    }
}

[System.Serializable]
public class InterSection
{
    public List<JunctionInfo> junctions = new List<JunctionInfo>();
    public void AddJunction(int sIdx, int kIdx, Spline s, BezierKnot k) => junctions.Add(new JunctionInfo(sIdx, kIdx, s, k));
    internal IEnumerable<JunctionInfo> GetJunctions() => junctions;
}

[System.Serializable]
public struct JunctionInfo
{
    public int splineIndex; public int knotIndex; public Spline spline; public BezierKnot knot;
    public JunctionInfo(int s, int k, Spline sp, BezierKnot kn) { splineIndex = s; knotIndex = k; spline = sp; knot = kn; }
}