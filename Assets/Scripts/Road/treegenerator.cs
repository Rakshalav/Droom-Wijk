using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(SplineContainer))]
public class SplineTreeManager : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject m_treePrefab;
    public float m_treeInterval = 15f;
    public float m_yOffset = 0f;

    [Header("Randomization")]
    public bool m_randomRotation = true;
    public float m_positionJitter = 0f;

    private bool m_needsUpdate = false;
    private void OnEnable() => Spline.Changed += OnSplineChanged;
    private void OnDisable() => Spline.Changed -= OnSplineChanged;
    private void OnSplineChanged(Spline s, int i, SplineModification m) => m_needsUpdate = true;
    private void OnValidate() => m_needsUpdate = true;

    private void Update()
    {
        if (m_needsUpdate && !Application.isPlaying)
        {
            PlaceTrees();
            m_needsUpdate = false;
        }
    }

    private void PlaceTrees()
    {
        ClearOldTrees();
        var container = GetComponent<SplineContainer>();
        if (container == null || m_treePrefab == null || m_treeInterval <= 0.1f) return;

        GameObject treeContainer = new GameObject("Trees_Container");
        treeContainer.transform.SetParent(transform, false);

        foreach (var spline in container.Splines)
        {
            float length = spline.GetLength();
            int treeCount = Mathf.FloorToInt(length / m_treeInterval);

            for (int i = 0; i <= treeCount; i++)
            {
                float t = (i * m_treeInterval) / length;
                spline.Evaluate(t, out float3 pos, out float3 tangent, out float3 up);

                Vector3 worldPos = (Vector3)pos;
                if (m_positionJitter > 0)
                {
                    Vector3 right = math.normalize(Vector3.Cross((Vector3)up, (Vector3)tangent));
                    worldPos += right * UnityEngine.Random.Range(-m_positionJitter, m_positionJitter);
                }
                worldPos += Vector3.up * m_yOffset;

                GameObject tree;
#if UNITY_EDITOR
                tree = (GameObject)PrefabUtility.InstantiatePrefab(m_treePrefab, treeContainer.transform);
#else
                tree = Instantiate(m_treePrefab, treeContainer.transform);
#endif

                tree.transform.localPosition = worldPos;
                tree.transform.localRotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)up);
                if (m_randomRotation) tree.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    private void ClearOldTrees()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name == "Trees_Container")
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}