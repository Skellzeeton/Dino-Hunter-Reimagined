using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class iTexture2D : MonoBehaviour
{
    public Vector3 m_v3Offset = Vector3.zero;
    public Material m_Material;
    public Vector2 m_v2Size = new Vector2(1f, 1f);

    protected MeshFilter m_MeshFilter;
    protected MeshRenderer m_MeshRenderer;

    private static readonly Dictionary<(Vector2 size, Vector3 offset), Mesh> s_MeshCache
    = new Dictionary<(Vector2, Vector3), Mesh>();
    private static bool s_Hooked;

    public void Start()
    {
        EnsureSceneHook();
        m_MeshFilter = gameObject.AddComponent<MeshFilter>();
        m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
        m_MeshRenderer.castShadows = false;
        m_MeshRenderer.receiveShadows = false;
        m_MeshRenderer.allowOcclusionWhenDynamic = false;
        m_MeshRenderer.lightProbeUsage = LightProbeUsage.Off;
        m_MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        m_MeshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
        m_MeshRenderer.sharedMaterial = m_Material;
        m_MeshFilter.sharedMesh = GetOrCreateMesh(m_v2Size, m_v3Offset);
    }

    private static void EnsureSceneHook()
    {
        if (s_Hooked) return;
        s_Hooked = true;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        foreach (var kvp in s_MeshCache)
        {
            if (kvp.Value != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(kvp.Value);
                else
                    Object.DestroyImmediate(kvp.Value);
            }
        }
        s_MeshCache.Clear();
    }

    private static Mesh GetOrCreateMesh(Vector2 size, Vector3 offset)
    {
        var key = (size, offset);
        if (s_MeshCache.TryGetValue(key, out Mesh cached) && cached != null)
            return cached;
        Mesh mesh = new Mesh();
        mesh.name = $"iTexture2D_{size.x}x{size.y}_{offset.x}_{offset.y}_{offset.z}";
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-size.x * 0.5f, 0f,  size.y * 0.5f) + offset,
            new Vector3(-size.x * 0.5f, 0f, -size.y * 0.5f) + offset,
            new Vector3( size.x * 0.5f, 0f, -size.y * 0.5f) + offset,
            new Vector3( size.x * 0.5f, 0f,  size.y * 0.5f) + offset
        };
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };
        int[] triangles = new int[6] { 0, 3, 1, 1, 3, 2 };
        mesh.vertices  = vertices;
        mesh.uv        = uv;
        mesh.normals   = new Vector3[0];
        mesh.triangles = triangles;
        mesh.UploadMeshData(true);
        s_MeshCache[key] = mesh;
        return mesh;
    }

    public void Show(bool bShow)
    {
        if (m_MeshRenderer != null && m_MeshRenderer.enabled != bShow)
            m_MeshRenderer.enabled = bShow;
    }
}