using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DroomwijkOrganicGenerator : EditorWindow
{
    [MenuItem("Tools/Droomwijk Organic Generator")]
    public static void ShowWindow() => GetWindow<DroomwijkOrganicGenerator>("Droomwijk Organic");

    public GameObject Row_Lowest, Row_Low, CourtYard, SemiDetached_Mid, SemiDetached_Upper;

    [Header("Organic Settings")]
    public Terrain terrain;
    public float minSpacing = 12f; // Breathing room + road space
    public float maxSpacing = 18f;
    public int targetBuildingCount = 100;

    [Header("Blender Fix")]
    public float modelRotationOffset = 0f; // Adjust if models face the wrong way

    private List<Vector3> placedPositions = new List<Vector3>();
    private int lowIncomeInRow = 0;

    void OnGUI()
    {
        GUILayout.Label("Droomwijk: Organic Growth Algorithm", EditorStyles.boldLabel);
        terrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain", terrain, typeof(Terrain), true);

        targetBuildingCount = EditorGUILayout.IntSlider("Building Count", targetBuildingCount, 10, 300);
        minSpacing = EditorGUILayout.FloatField("Min Spacing (Breathing Room)", minSpacing);
        maxSpacing = EditorGUILayout.FloatField("Max Spacing", maxSpacing);
        modelRotationOffset = EditorGUILayout.FloatField("Blender Y-Offset", modelRotationOffset);

        EditorGUILayout.Space();
        Row_Lowest = (GameObject)EditorGUILayout.ObjectField("Low (Lowest)", Row_Lowest, typeof(GameObject), false);
        Row_Low = (GameObject)EditorGUILayout.ObjectField("Low (Standard)", Row_Low, typeof(GameObject), false);
        CourtYard = (GameObject)EditorGUILayout.ObjectField("Low (Elderly)", CourtYard, typeof(GameObject), false);
        SemiDetached_Mid = (GameObject)EditorGUILayout.ObjectField("Middle Class", SemiDetached_Mid, typeof(GameObject), false);
        SemiDetached_Upper = (GameObject)EditorGUILayout.ObjectField("Free Sector", SemiDetached_Upper, typeof(GameObject), false);

        if (GUILayout.Button("Grow Organic Neighborhood")) GenerateOrganic();
        if (GUILayout.Button("Clear All")) ClearAll();
    }

    void GenerateOrganic()
    {
        if (terrain == null) return;

        placedPositions.Clear();
        lowIncomeInRow = 0;

        GameObject cityRoot = new GameObject("Organic_Droomwijk_" + System.DateTime.Now.Ticks);
        Vector3 startPos = SceneView.lastActiveSceneView.pivot;

        // Use a Queue for a "Breadth-First" organic growth
        Queue<Vector3> growthPoints = new Queue<Vector3>();
        growthPoints.Enqueue(startPos);
        placedPositions.Add(startPos);

        int spawnedCount = 0;
        int attempts = 0;

        while (spawnedCount < targetBuildingCount && growthPoints.Count > 0 && attempts < 2000)
        {
            attempts++;
            Vector3 currentParent = growthPoints.Dequeue();

            // Try to spawn 3-5 neighbors around this point
            int neighbors = Random.Range(2, 5);
            for (int i = 0; i < neighbors; i++)
            {
                if (spawnedCount >= targetBuildingCount) break;

                // Pick a random angle and random distance (Organic placement)
                float angle = Random.Range(0f, 360f);
                float distance = Random.Range(minSpacing, maxSpacing);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
                Vector3 potentialPos = currentParent + offset;

                if (IsValidPosition(potentialPos))
                {
                    SpawnBuilding(potentialPos, cityRoot.transform, currentParent);
                    placedPositions.Add(potentialPos);
                    growthPoints.Enqueue(potentialPos);
                    spawnedCount++;
                }
            }
        }
        Undo.RegisterCreatedObjectUndo(cityRoot, "Organic Growth");
    }

    bool IsValidPosition(Vector3 pos)
    {
        foreach (Vector3 p in placedPositions)
        {
            if (Vector3.Distance(pos, p) < minSpacing) return false;
        }
        return true;
    }

    void SpawnBuilding(Vector3 pos, Transform parent, Vector3 lookAtPoint)
    {
        GameObject prefab = PickMixedPrefab();
        float y = terrain.SampleHeight(pos) + terrain.transform.position.y;
        Vector3 finalPos = new Vector3(pos.x, y, pos.z);

        GameObject house = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        house.transform.position = finalPos;
        house.transform.parent = parent;

        // ORGANIC ROTATION: Face generally toward the "parent" or slightly randomized
        Vector3 directionToCenter = (lookAtPoint - pos).normalized;
        float randomYaw = Random.Range(-20f, 20f); // Slight variation
        house.transform.rotation = Quaternion.LookRotation(directionToCenter) * Quaternion.Euler(0, modelRotationOffset + randomYaw, 0);
    }

    GameObject PickMixedPrefab()
    {
        float rnd = Random.value;
        // Anti-Segregation: 30% Low Income
        if (rnd < 0.30f && lowIncomeInRow < 2)
        {
            lowIncomeInRow++;
            float typeRnd = Random.value;
            if (typeRnd < 0.33f) return Row_Lowest;
            if (typeRnd < 0.66f) return Row_Low;
            return CourtYard;
        }

        lowIncomeInRow = 0;
        return (rnd < 0.70f) ? SemiDetached_Mid : SemiDetached_Upper;
    }

    void ClearAll()
    {
        var all = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in all) if (go.name.StartsWith("Organic_Droomwijk")) Undo.DestroyObjectImmediate(go);
    }
}