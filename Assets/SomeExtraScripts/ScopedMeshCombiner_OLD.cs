/*using UnityEngine;
using System.Collections.Generic;

public class ScopedMeshCombiner_OLD : MonoBehaviour
{
    public bool combineOnStart = true;
    public bool castShadows = false;
    public bool receiveShadows = false;

    void Start()
    {
        if (combineOnStart)
            CombineChildMeshes();
    }

    public void CombineChildMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> renderCombine = new List<CombineInstance>();
        List<CombineInstance> colliderCombine = new List<CombineInstance>();
        List<CombineInstance> barrierCombine = new List<CombineInstance>();
        List<CombineInstance> barrierColliderCombine = new List<CombineInstance>();
        List<Material> materials = new List<Material>();
        List<int> materialIndices = new List<int>();
        Dictionary<Material, int> materialLookup = new Dictionary<Material, int>();
        List<GameObject> toDestroy = new List<GameObject>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            Transform tf = meshFilters[i].transform;
            if (tf == transform) continue;
            if (tf.GetComponent<SkinnedMeshRenderer>() != null) continue;
            if (tf.GetComponent<BoxCollider>() != null) continue;

            MeshRenderer renderer = tf.GetComponent<MeshRenderer>();
            if (renderer == null || meshFilters[i].sharedMesh == null)
                continue;

            Mesh mesh = meshFilters[i].sharedMesh;
            Material[] meshMats = renderer.sharedMaterials;
            bool hasMeshCollider = tf.GetComponent<MeshCollider>() != null;
            int layer = tf.gameObject.layer;

            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                Material mat = (meshMats.Length > sub) ? meshMats[sub] : (meshMats.Length > 0 ? meshMats[0] : null);
                if (mat == null && layer != 28) continue;

                CombineInstance ci = new CombineInstance();
                ci.mesh = mesh;
                ci.subMeshIndex = sub;
                ci.transform = tf.localToWorldMatrix;

                if (layer == 28)
                {
                    barrierCombine.Add(ci);
                    barrierColliderCombine.Add(ci);
                    continue;
                }

                int matIndex;
                if (!materialLookup.ContainsKey(mat))
                {
                    matIndex = materials.Count;
                    materials.Add(mat);
                    materialLookup.Add(mat, matIndex);
                }
                else
                {
                    matIndex = materialLookup[mat];
                }

                renderCombine.Add(ci);
                materialIndices.Add(matIndex);

                if (hasMeshCollider)
                {
                    CombineInstance colliderCI = new CombineInstance();
                    colliderCI.mesh = mesh;
                    colliderCI.subMeshIndex = sub;
                    colliderCI.transform = tf.localToWorldMatrix;
                    colliderCombine.Add(colliderCI);
                }
            }

            toDestroy.Add(tf.gameObject);
        }

        // Group render meshes by material
        List<Mesh> subMeshes = new List<Mesh>();
        List<Material> finalMaterials = new List<Material>();

        for (int m = 0; m < materials.Count; m++)
        {
            List<CombineInstance> subCombine = new List<CombineInstance>();
            for (int j = 0; j < renderCombine.Count; j++)
            {
                if (materialIndices[j] == m)
                    subCombine.Add(renderCombine[j]);
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(subCombine.ToArray(), true, true);
            subMeshes.Add(combinedMesh);
            finalMaterials.Add(materials[m]);
        }

        // Final render mesh
        CombineInstance[] finalCombine = new CombineInstance[subMeshes.Count];
        for (int i = 0; i < subMeshes.Count; i++)
        {
            finalCombine[i].mesh = subMeshes[i];
            finalCombine[i].subMeshIndex = 0;
            finalCombine[i].transform = Matrix4x4.identity;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.name = "CombinedMesh";
        finalMesh.CombineMeshes(finalCombine, false, false);

        GameObject combinedGO = new GameObject("CombinedMesh");
        combinedGO.transform.parent = transform;
        combinedGO.transform.localPosition = Vector3.zero;
        combinedGO.transform.localRotation = Quaternion.identity;
        combinedGO.transform.localScale = Vector3.one;
        combinedGO.layer = 31;

        MeshFilter mf = combinedGO.AddComponent<MeshFilter>();
        mf.sharedMesh = finalMesh;

        MeshRenderer mr = combinedGO.AddComponent<MeshRenderer>();
        mr.sharedMaterials = finalMaterials.ToArray();
        mr.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = receiveShadows;
        mr.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
        mr.allowOcclusionWhenDynamic = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        // Final collider mesh
        if (colliderCombine.Count > 0)
        {
            Mesh colliderMesh = new Mesh();
            colliderMesh.name = "CombinedColliderMesh";
            colliderMesh.CombineMeshes(colliderCombine.ToArray(), false, true);

            MeshCollider mc = combinedGO.AddComponent<MeshCollider>();
            mc.sharedMesh = colliderMesh;
            mc.convex = false;
        }

        // Barrier mesh (layer 28) — no renderer, just collider
        if (barrierCombine.Count > 0)
        {
            Mesh barrierMesh = new Mesh();
            barrierMesh.name = "BarrierMesh_L28";
            barrierMesh.CombineMeshes(barrierCombine.ToArray(), true, true);

            GameObject barrierGO = new GameObject("BarrierMesh_L28");
            barrierGO.transform.parent = transform;
            barrierGO.transform.localPosition = Vector3.zero;
            barrierGO.transform.localRotation = Quaternion.identity;
            barrierGO.transform.localScale = Vector3.one;
            barrierGO.layer = 28;

            MeshFilter barrierMF = barrierGO.AddComponent<MeshFilter>();
            barrierMF.sharedMesh = barrierMesh;

            if (barrierColliderCombine.Count > 0)
            {
                Mesh barrierColliderMesh = new Mesh();
                barrierColliderMesh.name = "BarrierColliderMesh_L28";
                barrierColliderMesh.CombineMeshes(barrierColliderCombine.ToArray(), false, true);

                MeshCollider barrierMC = barrierGO.AddComponent<MeshCollider>();
                barrierMC.sharedMesh = barrierColliderMesh;
                barrierMC.convex = false;
            }
        }

        // Cleanup originals
        for (int i = 0; i < toDestroy.Count; i++)
        {
            Destroy(toDestroy[i]);
        }
    }
}*/

//newerr version lool
/*using UnityEngine;
using System.Collections.Generic;

public class ScopedMeshCombiner : MonoBehaviour
{
    public bool combineOnStart = true;
    public bool castShadows = false;
    public bool receiveShadows = false;

    void Start()
    {
        if (combineOnStart)
            CombineChildMeshes();
    }

    public void CombineChildMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> renderCombine = new List<CombineInstance>();
        List<CombineInstance> colliderCombine = new List<CombineInstance>();
        List<CombineInstance> barrierCombine = new List<CombineInstance>();
        List<CombineInstance> barrierColliderCombine = new List<CombineInstance>();
        List<CombineInstance> groundCombine = new List<CombineInstance>();
        List<CombineInstance> groundColliderCombine = new List<CombineInstance>();
        List<Material> materials = new List<Material>();
        List<int> materialIndices = new List<int>();
        Dictionary<Material, int> materialLookup = new Dictionary<Material, int>();
        List<GameObject> toDestroy = new List<GameObject>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            Transform tf = meshFilters[i].transform;
            if (tf == transform) continue;
            if (tf.GetComponent<SkinnedMeshRenderer>() != null) continue;
            if (tf.GetComponent<BoxCollider>() != null) continue;

            MeshRenderer renderer = tf.GetComponent<MeshRenderer>();
            if (renderer == null || meshFilters[i].sharedMesh == null)
                continue;

            Mesh mesh = meshFilters[i].sharedMesh;
            Material[] meshMats = renderer.sharedMaterials;
            bool hasMeshCollider = tf.GetComponent<MeshCollider>() != null;
            int layer = tf.gameObject.layer;

            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                Material mat = (meshMats.Length > sub) ? meshMats[sub] : (meshMats.Length > 0 ? meshMats[0] : null);
                if (mat == null && layer != 28 && layer != 29) continue;

                CombineInstance ci = new CombineInstance();
                ci.mesh = mesh;
                ci.subMeshIndex = sub;
                ci.transform = tf.localToWorldMatrix;

                if (layer == 28)
                {
                    barrierCombine.Add(ci);
                    barrierColliderCombine.Add(ci);
                    continue;
                }

                // Fuse layer 29 into main combined mesh
                if (layer == 29 || layer == 31)
                {
                    int matIndex;
                    if (!materialLookup.ContainsKey(mat))
                    {
                        matIndex = materials.Count;
                        materials.Add(mat);
                        materialLookup.Add(mat, matIndex);
                    }
                    else
                    {
                        matIndex = materialLookup[mat];
                    }

                    renderCombine.Add(ci);
                    materialIndices.Add(matIndex);

                    if (hasMeshCollider)
                    {
                        CombineInstance colliderCI = new CombineInstance();
                        colliderCI.mesh = mesh;
                        colliderCI.subMeshIndex = sub;
                        colliderCI.transform = tf.localToWorldMatrix;
                        colliderCombine.Add(colliderCI);
                    }

                    continue;
                }
            }

            toDestroy.Add(tf.gameObject);
        }

        // Combine render meshes by material
        List<Mesh> subMeshes = new List<Mesh>();
        List<Material> finalMaterials = new List<Material>();

        for (int m = 0; m < materials.Count; m++)
        {
            List<CombineInstance> subCombine = new List<CombineInstance>();
            for (int j = 0; j < renderCombine.Count; j++)
            {
                if (materialIndices[j] == m)
                    subCombine.Add(renderCombine[j]);
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(subCombine.ToArray(), true, true);
            subMeshes.Add(combinedMesh);
            finalMaterials.Add(materials[m]);
        }

        CombineInstance[] finalCombine = new CombineInstance[subMeshes.Count];
        for (int i = 0; i < subMeshes.Count; i++)
        {
            finalCombine[i].mesh = subMeshes[i];
            finalCombine[i].subMeshIndex = 0;
            finalCombine[i].transform = Matrix4x4.identity;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.name = "CombinedMesh";
        finalMesh.CombineMeshes(finalCombine, false, false);

        GameObject combinedGO = new GameObject("CombinedMesh");
        combinedGO.transform.parent = transform;
        combinedGO.transform.localPosition = Vector3.zero;
        combinedGO.transform.localRotation = Quaternion.identity;
        combinedGO.transform.localScale = Vector3.one;
        combinedGO.layer = 31;

        MeshFilter mf = combinedGO.AddComponent<MeshFilter>();
        mf.sharedMesh = finalMesh;

        MeshRenderer mr = combinedGO.AddComponent<MeshRenderer>();
        mr.sharedMaterials = finalMaterials.ToArray();
        mr.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = receiveShadows;
        mr.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
        mr.allowOcclusionWhenDynamic = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        if (colliderCombine.Count > 0)
        {
            Mesh colliderMesh = new Mesh();
            colliderMesh.name = "CombinedColliderMesh";
            colliderMesh.CombineMeshes(colliderCombine.ToArray(), false, true);

            MeshCollider mc = combinedGO.AddComponent<MeshCollider>();
            mc.sharedMesh = colliderMesh;
            mc.convex = false;
        }

        // Barrier mesh (layer 28)
        if (barrierCombine.Count > 0)
        {
            Mesh barrierMesh = new Mesh();
            barrierMesh.name = "BarrierMesh_L28";
            barrierMesh.CombineMeshes(barrierCombine.ToArray(), true, true);

            GameObject barrierGO = new GameObject("BarrierMesh_L28");
            barrierGO.transform.parent = transform;
            barrierGO.transform.localPosition = Vector3.zero;
            barrierGO.transform.localRotation = Quaternion.identity;
            barrierGO.transform.localScale = Vector3.one;
            barrierGO.layer = 28;

            MeshFilter barrierMF = barrierGO.AddComponent<MeshFilter>();
            barrierMF.sharedMesh = barrierMesh;

            if (barrierColliderCombine.Count > 0)
            {
                Mesh barrierColliderMesh = new Mesh();
                barrierColliderMesh.name = "BarrierColliderMesh_L28";
                barrierColliderMesh.CombineMeshes(barrierColliderCombine.ToArray(), false, true);

                MeshCollider barrierMC = barrierGO.AddComponent<MeshCollider>();
                barrierMC.sharedMesh = barrierColliderMesh;
                barrierMC.convex = false;
            }
        }

        /* Uncomment to separate ground mesh (layer 29)
        if (groundCombine.Count > 0)
        {
            Mesh groundMesh = new Mesh();
            groundMesh.name = "Ground";
            groundMesh.CombineMeshes(groundCombine.ToArray(), true, true);

            GameObject groundGO = new GameObject("Ground");
            groundGO.transform.parent = transform;
            groundGO.transform.localPosition = Vector3.zero;
            groundGO.transform.localRotation = Quaternion.identity;
            groundGO.transform.localScale = Vector3.one;
            groundGO.layer = 29;

            MeshFilter groundMF = groundGO.AddComponent<MeshFilter>();
            groundMF.sharedMesh = groundMesh;

            MeshRenderer groundMR = groundGO.AddComponent<MeshRenderer>();
            groundMR.sharedMaterials = groundMaterials.ToArray();
            groundMR.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            groundMR.receiveShadows = receiveShadows;
            groundMR.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
            groundMR.allowOcclusionWhenDynamic = false;
            groundMR.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            groundMR.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            if (groundColliderCombine.Count > 0)
            {
                Mesh groundColliderMesh = new Mesh();
                groundColliderMesh.name = "GroundColliderMesh";
                groundColliderMesh.CombineMeshes(groundColliderCombine.ToArray(), false, true);

                MeshCollider groundMC = groundGO.AddComponent<MeshCollider>();
                groundMC.sharedMesh = groundColliderMesh;
                groundMC.convex = false;
            }
        }
        

        for (int i = 0; i < toDestroy.Count; i++)
            Destroy(toDestroy[i]);
    }
}*/

