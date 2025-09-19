using UnityEngine;
using UnityEditor;

public class ApplySelectedPrefabs : EditorWindow
{
    [MenuItem("Tools/Apply Changes to Selected Prefabs")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ApplySelectedPrefabs), false, "Apply Prefabs");
    }

    private void OnGUI()
    {
        GUILayout.Label("Apply Changes to Selected Prefab Instances", EditorStyles.boldLabel);

        if (GUILayout.Button("Apply to All Selected"))
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            int appliedCount = 0;

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                GameObject go = selectedObjects[i];
                GameObject prefabRoot = PrefabUtility.GetPrefabParent(go) as GameObject;

                if (prefabRoot == null)
                {
                    Debug.LogWarning("Skipping non-prefab object: " + go.name);
                    continue;
                }

                PrefabUtility.ReplacePrefab(go, prefabRoot, ReplacePrefabOptions.ConnectToPrefab);
                appliedCount++;
            }

            Debug.Log("Applied changes to " + appliedCount + " prefab instances.");
        }
    }
}