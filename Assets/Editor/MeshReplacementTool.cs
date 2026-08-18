using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MeshReplacementTool : EditorWindow
{
    private GameObject targetObject;
    private List<GameObject> fbxObjects = new List<GameObject>();
    private bool matchByName = true;
    private bool matchByPath = false;
    private bool matchByMeshName = false;
    private bool replaceMeshFilters = true;
    private bool replaceMeshColliders = true;
    private bool includeInactive = true;
    private bool createBackup = true;
    private bool normalizeNameDifferences = true;

    [MenuItem("Tools/Mesh Replacement Tool")]
    public static void ShowWindow()
    {
        GetWindow<MeshReplacementTool>("Mesh Replacement Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mesh Replacement Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Select the target GameObject/Prefab and one or more FBX files. Meshes will be replaced wherever a match is found.", MessageType.Info);
        GUILayout.Space(10);

        // Target object
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        GUILayout.Space(5);

        // FBX objects list
        EditorGUILayout.LabelField("FBX Objects (Sources)", EditorStyles.boldLabel);
        for (int i = 0; i < fbxObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            fbxObjects[i] = (GameObject)EditorGUILayout.ObjectField($"FBX {i + 1}", fbxObjects[i], typeof(GameObject), true);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                fbxObjects.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add FBX Object"))
        {
            fbxObjects.Add(null);
        }

        GUILayout.Space(10);

        // Matching options
        EditorGUILayout.LabelField("Matching Options", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        matchByName = EditorGUILayout.Toggle("Match by Object Name", matchByName);
        matchByPath = EditorGUILayout.Toggle("Match by Relative Path", matchByPath);
        matchByMeshName = EditorGUILayout.Toggle("Match by Mesh Name", matchByMeshName);
        if (EditorGUI.EndChangeCheck())
        {
            // Ensure at least one matching method is selected
            if (!matchByName && !matchByPath && !matchByMeshName)
                matchByName = true;
        }

        normalizeNameDifferences = EditorGUILayout.Toggle("Normalize Spaces/Underscores", normalizeNameDifferences);

        GUILayout.Space(10);

        // Replacement options
        EditorGUILayout.LabelField("Replacement Options", EditorStyles.boldLabel);
        replaceMeshFilters = EditorGUILayout.Toggle("Replace Mesh Filters", replaceMeshFilters);
        replaceMeshColliders = EditorGUILayout.Toggle("Replace Mesh Colliders", replaceMeshColliders);
        includeInactive = EditorGUILayout.Toggle("Include Inactive Objects", includeInactive);
        createBackup = EditorGUILayout.Toggle("Create Backup Before Replace", createBackup);

        GUILayout.Space(20);

        bool hasValidTarget = targetObject != null;
        bool hasValidFbx = fbxObjects.Any(f => f != null);
        EditorGUI.BeginDisabledGroup(!hasValidTarget || !hasValidFbx);

        if (GUILayout.Button("Replace Meshes", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Mesh Replacement",
                $"Replace meshes in '{targetObject.name}' using {fbxObjects.Count(f => f != null)} FBX source(s)?",
                "Yes, Replace", "Cancel"))
            {
                ReplaceMeshes();
            }
        }

        if (GUILayout.Button("Preview Changes", GUILayout.Height(30)))
        {
            PreviewChanges();
        }

        EditorGUI.EndDisabledGroup();
    }

    private void ReplaceMeshes()
    {
        if (targetObject == null || !fbxObjects.Any(f => f != null))
        {
            EditorUtility.DisplayDialog("Error", "Please assign a target object and at least one FBX object.", "OK");
            return;
        }

        // Create backup if requested
        GameObject backupObject = null;
        if (createBackup)
        {
            backupObject = Instantiate(targetObject);
            backupObject.name = targetObject.name + "_Backup";
            backupObject.transform.SetParent(targetObject.transform.parent);
            backupObject.transform.localPosition = targetObject.transform.localPosition;
            backupObject.transform.localRotation = targetObject.transform.localRotation;
            backupObject.transform.localScale = targetObject.transform.localScale;
            backupObject.SetActive(false);
            Undo.RegisterCreatedObjectUndo(backupObject, "Create Backup");
        }

        // Collect all source meshes from all FBX objects
        var sourceMeshFilters = new List<MeshFilter>();
        var sourceMeshColliders = new List<MeshCollider>();

        foreach (var fbx in fbxObjects)
        {
            if (fbx == null) continue;
            sourceMeshFilters.AddRange(GetComponentsIncludingInactive<MeshFilter>(fbx));
            sourceMeshColliders.AddRange(GetComponentsIncludingInactive<MeshCollider>(fbx));
        }

        // Build dictionaries for matching, using selected methods
        var filterDict = new Dictionary<string, MeshFilter>();
        var colliderDict = new Dictionary<string, MeshCollider>();

        foreach (var filter in sourceMeshFilters)
        {
            if (filter.sharedMesh == null) continue;
            var keys = GetMatchingKeys(filter.gameObject, filter.sharedMesh.name);
            foreach (var key in keys)
            {
                if (!filterDict.ContainsKey(key))
                    filterDict[key] = filter;
            }
        }

        foreach (var collider in sourceMeshColliders)
        {
            if (collider.sharedMesh == null) continue;
            var keys = GetMatchingKeys(collider.gameObject, collider.sharedMesh.name);
            foreach (var key in keys)
            {
                if (!colliderDict.ContainsKey(key))
                    colliderDict[key] = collider;
            }
        }

        // Get target components
        var targetMeshFilters = GetComponentsIncludingInactive<MeshFilter>(targetObject);
        var targetMeshColliders = GetComponentsIncludingInactive<MeshCollider>(targetObject);

        int replacedCount = 0;
        int skippedCount = 0;
        var skippedObjects = new List<string>();

        // Replace Mesh Filters
        if (replaceMeshFilters)
        {
            foreach (var targetFilter in targetMeshFilters)
            {
                var keys = GetMatchingKeys(targetFilter.gameObject, targetFilter.sharedMesh != null ? targetFilter.sharedMesh.name : "");
                MeshFilter sourceFilter = null;
                foreach (var key in keys)
                {
                    if (filterDict.TryGetValue(key, out sourceFilter))
                        break;
                }

                if (sourceFilter != null)
                {
                    if (targetFilter.sharedMesh != sourceFilter.sharedMesh)
                    {
                        Undo.RecordObject(targetFilter, "Replace Mesh Filter");
                        targetFilter.sharedMesh = sourceFilter.sharedMesh;
                        replacedCount++;
                    }
                    else
                    {
                        skippedCount++;
                        skippedObjects.Add($"{GetPath(targetFilter.gameObject)} (Mesh Filter - Same mesh)");
                    }
                }
                else
                {
                    skippedCount++;
                    skippedObjects.Add($"{GetPath(targetFilter.gameObject)} (Mesh Filter - No match)");
                }
            }
        }

        // Replace Mesh Colliders
        if (replaceMeshColliders)
        {
            foreach (var targetCollider in targetMeshColliders)
            {
                var keys = GetMatchingKeys(targetCollider.gameObject, targetCollider.sharedMesh != null ? targetCollider.sharedMesh.name : "");
                MeshCollider sourceCollider = null;
                foreach (var key in keys)
                {
                    if (colliderDict.TryGetValue(key, out sourceCollider))
                        break;
                }

                // If no collider match, try mesh filter match
                if (sourceCollider == null)
                {
                    foreach (var key in keys)
                    {
                        if (filterDict.TryGetValue(key, out MeshFilter sourceFilter))
                        {
                            if (sourceFilter.sharedMesh != null)
                            {
                                if (targetCollider.sharedMesh != sourceFilter.sharedMesh)
                                {
                                    Undo.RecordObject(targetCollider, "Replace Mesh Collider");
                                    targetCollider.sharedMesh = sourceFilter.sharedMesh;
                                    replacedCount++;
                                    sourceCollider = null; // mark as replaced
                                }
                                else
                                {
                                    skippedCount++;
                                    skippedObjects.Add($"{GetPath(targetCollider.gameObject)} (Mesh Collider - Same mesh)");
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (targetCollider.sharedMesh != sourceCollider.sharedMesh)
                    {
                        Undo.RecordObject(targetCollider, "Replace Mesh Collider");
                        targetCollider.sharedMesh = sourceCollider.sharedMesh;
                        replacedCount++;
                    }
                    else
                    {
                        skippedCount++;
                        skippedObjects.Add($"{GetPath(targetCollider.gameObject)} (Mesh Collider - Same mesh)");
                    }
                }
            }
        }

        EditorUtility.SetDirty(targetObject);

        string message = $"Replacement Complete!\n\n" +
                        $"Replaced: {replacedCount} component(s)\n" +
                        $"Skipped: {skippedCount} component(s)\n\n";

        if (skippedObjects.Count > 0)
        {
            message += "Skipped Objects (first 10):\n";
            for (int i = 0; i < Mathf.Min(10, skippedObjects.Count); i++)
            {
                message += $"• {skippedObjects[i]}\n";
            }
            if (skippedObjects.Count > 10)
                message += $"... and {skippedObjects.Count - 10} more";
        }

        if (createBackup && backupObject != null)
            message += $"\n\nBackup created: {backupObject.name} (inactive)";

        EditorUtility.DisplayDialog("Mesh Replacement Results", message, "OK");
    }

    private void PreviewChanges()
    {
        if (targetObject == null || !fbxObjects.Any(f => f != null))
        {
            EditorUtility.DisplayDialog("Error", "Please assign a target object and at least one FBX object.", "OK");
            return;
        }

        // Collect source meshes
        var sourceMeshFilters = new List<MeshFilter>();
        var sourceMeshColliders = new List<MeshCollider>();

        foreach (var fbx in fbxObjects)
        {
            if (fbx == null) continue;
            sourceMeshFilters.AddRange(GetComponentsIncludingInactive<MeshFilter>(fbx));
            sourceMeshColliders.AddRange(GetComponentsIncludingInactive<MeshCollider>(fbx));
        }

        // Build dictionaries similar to replace
        var filterDict = new Dictionary<string, MeshFilter>();
        foreach (var filter in sourceMeshFilters)
        {
            if (filter.sharedMesh == null) continue;
            var keys = GetMatchingKeys(filter.gameObject, filter.sharedMesh.name);
            foreach (var key in keys)
            {
                if (!filterDict.ContainsKey(key))
                    filterDict[key] = filter;
            }
        }

        var targetMeshFilters = GetComponentsIncludingInactive<MeshFilter>(targetObject);
        var targetMeshColliders = GetComponentsIncludingInactive<MeshCollider>(targetObject);

        int possibleReplacements = 0;
        var previewList = new List<string>();

        foreach (var targetFilter in targetMeshFilters)
        {
            var keys = GetMatchingKeys(targetFilter.gameObject, targetFilter.sharedMesh != null ? targetFilter.sharedMesh.name : "");
            bool found = keys.Any(k => filterDict.ContainsKey(k));
            if (found)
            {
                possibleReplacements++;
                previewList.Add($"✓ {GetPath(targetFilter.gameObject)}: Mesh Filter will be replaced");
            }
        }

        foreach (var targetCollider in targetMeshColliders)
        {
            var keys = GetMatchingKeys(targetCollider.gameObject, targetCollider.sharedMesh != null ? targetCollider.sharedMesh.name : "");
            bool found = keys.Any(k => filterDict.ContainsKey(k) || sourceMeshColliders.Any(c => GetMatchingKeys(c.gameObject, c.sharedMesh.name).Contains(k)));
            if (found)
            {
                possibleReplacements++;
                previewList.Add($"✓ {GetPath(targetCollider.gameObject)}: Mesh Collider will be replaced");
            }
        }

        string message = $"Preview: {possibleReplacements} potential replacement(s)\n\n";
        foreach (var item in previewList.Take(20))
        {
            message += item + "\n";
        }
        if (previewList.Count > 20)
            message += $"... and {previewList.Count - 20} more";

        EditorUtility.DisplayDialog("Preview Changes", message, "OK");
    }

    // Returns a list of possible keys for matching based on selected options
    private List<string> GetMatchingKeys(GameObject obj, string meshName)
    {
        var keys = new List<string>();
        if (matchByName)
        {
            AddKeyWithNormalization(keys, obj.name);
        }
        if (matchByPath)
        {
            string path = GetRelativePath(obj);
            AddKeyWithNormalization(keys, path);
        }
        if (matchByMeshName && !string.IsNullOrEmpty(meshName))
        {
            AddKeyWithNormalization(keys, meshName);
        }
        return keys;
    }

    private void AddKeyWithNormalization(List<string> keys, string original)
    {
        if (string.IsNullOrEmpty(original)) return;
        keys.Add(original);

        if (normalizeNameDifferences)
        {
            // Replace spaces with underscores
            string spacesToUnderscores = original.Replace(' ', '_');
            if (spacesToUnderscores != original)
                keys.Add(spacesToUnderscores);

            // Replace underscores with spaces
            string underscoresToSpaces = original.Replace('_', ' ');
            if (underscoresToSpaces != original)
                keys.Add(underscoresToSpaces);
        }
    }

    private string GetRelativePath(GameObject obj)
    {
        if (obj == null) return "";
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private string GetPath(GameObject obj)
    {
        return GetRelativePath(obj);
    }

    private T[] GetComponentsIncludingInactive<T>(GameObject obj) where T : Component
    {
        if (includeInactive)
            return obj.GetComponentsInChildren<T>(true);
        else
            return obj.GetComponentsInChildren<T>(false);
    }
}