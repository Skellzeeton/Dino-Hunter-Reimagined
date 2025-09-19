using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderOverrideManager : MonoBehaviour
{
    [System.Serializable]
    public class ShaderMapping
    {
        public Shader sourceShader;
        public Shader replacementShader;
    }

    [Tooltip("List of shaders to replace and their replacements. If only one replacement is set, it will be used for all.")]
    public List<ShaderMapping> shaderMappings = new List<ShaderMapping>();

    private Dictionary<Material, Material> replacedMaterials = new Dictionary<Material, Material>();
    private Dictionary<Shader, Shader> shaderMap = new Dictionary<Shader, Shader>();

    private void Start()
    {
        BuildShaderMap();
        StartCoroutine(CheckAndReplaceRoutine());
    }

    private void BuildShaderMap()
    {
        shaderMap.Clear();

        if (shaderMappings.Count == 0)
        {
            Debug.LogWarning("ShaderOverrideManager has no shader mappings assigned.");
            return;
        }

        // If only one replacement is set, use it for all source shaders
        bool useSingleReplacement = true;
        Shader singleReplacement = null;

        foreach (var mapping in shaderMappings)
        {
            if (mapping.replacementShader != null)
            {
                if (singleReplacement == null)
                    singleReplacement = mapping.replacementShader;
                else if (singleReplacement != mapping.replacementShader)
                    useSingleReplacement = false;
            }
        }

        foreach (var mapping in shaderMappings)
        {
            if (mapping.sourceShader == null)
                continue;

            Shader target = mapping.replacementShader;

            if (useSingleReplacement && singleReplacement != null)
                target = singleReplacement;

            if (target != null)
                shaderMap[mapping.sourceShader] = target;
        }
    }

    private IEnumerator CheckAndReplaceRoutine()
    {
        while (true)
        {
            ReplaceShadersInScene();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ReplaceShadersInScene()
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            bool modified = false;

            for (int i = 0; i < materials.Length; i++)
            {
                Material original = materials[i];
                if (original == null || original.shader == null)
                    continue;

                Shader replacement;
                if (shaderMap.TryGetValue(original.shader, out replacement))
                {
                    if (!replacedMaterials.ContainsKey(original))
                    {
                        Material newMat = new Material(original);
                        newMat.shader = replacement;
                        replacedMaterials[original] = newMat;
                    }

                    materials[i] = replacedMaterials[original];
                    modified = true;
                }
            }

            if (modified)
            {
                renderer.sharedMaterials = materials;
            }
        }
    }

    private void OnDestroy()
    {
        CleanupMaterials();
    }

    private void OnDisable()
    {
        CleanupMaterials();
    }

    private void CleanupMaterials()
    {
        foreach (var mat in replacedMaterials.Values)
        {
            if (mat != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(mat);
                else
                    DestroyImmediate(mat);
#else
                Destroy(mat);
#endif
            }
        }

        replacedMaterials.Clear();
    }
}
