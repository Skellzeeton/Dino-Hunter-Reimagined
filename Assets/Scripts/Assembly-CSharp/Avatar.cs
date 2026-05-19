using UnityEngine;

public class Avatar : MonoBehaviour
{
    public GameObject[] m_AvatarPart;

    protected GameObject[] m_AvatarEffect;
    
    protected GameObject[] m_AvatarAccessories;

    private void Awake()
    {
       if (m_AvatarPart == null)
       {
          return;
       }
       m_AvatarEffect = new GameObject[m_AvatarPart.Length];
       m_AvatarAccessories = new GameObject[m_AvatarPart.Length];
       
       for (int i = 0; i < m_AvatarPart.Length; i++)
       {
          if (m_AvatarPart[i] == null)
          {
             continue;
          }
          SkinnedMeshRenderer component = m_AvatarPart[i].GetComponent<SkinnedMeshRenderer>();
          if (component != null)
          {
             {
                component.sharedMaterial = new Material(component.material);
             }
          }
       }
    }

    public void ReplaceAvatarEffect(int nIndex, GameObject effectprefab)
    {
       if (nIndex < 0 || nIndex >= m_AvatarEffect.Length)
       {
          return;
       }
       if (m_AvatarEffect[nIndex] != null)
       {
          Object.Destroy(m_AvatarEffect[nIndex]);
       }
       if (!(effectprefab == null))
       {
          GameObject gameObject = Object.Instantiate(effectprefab, m_AvatarPart[nIndex].transform.position, Quaternion.identity) as GameObject;
          if (!(gameObject == null))
          {
             gameObject.transform.parent = m_AvatarPart[nIndex].transform;
             gameObject.transform.localPosition = Vector3.zero;
             gameObject.transform.localRotation = Quaternion.identity;
             m_AvatarEffect[nIndex] = gameObject;
          }
       }
    }
   

    public void ReplaceAvatar(int nIndex, string sPath_Prefab, Texture texture)
    {
       GameObject newpartprefab = Resources.Load(sPath_Prefab) as GameObject;
       ReplaceAvatar(nIndex, newpartprefab, texture);
    }
   

    public void ReplaceAvatar(int nIndex, GameObject newpartprefab, Texture texture)
    {
       if (m_AvatarPart == null || newpartprefab == null || nIndex < 0 || nIndex >= m_AvatarPart.Length)
       {
          return;
       }
       if (nIndex >= 3 && nIndex <= 6)
       {
          ReplaceAccessory(nIndex, newpartprefab, texture);
          return;
       }
       GameObject gameObject = m_AvatarPart[nIndex];
       if (gameObject == null)
       {
          return;
       }
       SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
       if (component == null)
       {
          return;
       }
       GameObject gameObject2 = Object.Instantiate(newpartprefab, Vector3.zero, Quaternion.identity) as GameObject;
       if (!(gameObject2 == null))
       {
          SkinnedMeshRenderer componentInChildren = gameObject2.GetComponentInChildren<SkinnedMeshRenderer>();
          Replace(component, componentInChildren);
          component.sharedMaterial.mainTexture = texture;
          Object.Destroy(gameObject2);
       }
    }

    protected void ReplaceAccessory(int nIndex, GameObject accessoryPrefab, Texture texture)
    {
       if (m_AvatarAccessories == null || nIndex < 0 || nIndex >= m_AvatarAccessories.Length)
       {
          return;
       }
       if (m_AvatarAccessories[nIndex] != null)
       {
          Object.Destroy(m_AvatarAccessories[nIndex]);
          m_AvatarAccessories[nIndex] = null;
       }
       if (accessoryPrefab == null)
       {
          return;
       }
       GameObject gameObject = m_AvatarPart[nIndex];
       if (gameObject == null)
       {
          return;
       }
       GameObject accessoryInstance = Object.Instantiate(accessoryPrefab, Vector3.zero, Quaternion.identity) as GameObject;
       if (accessoryInstance != null)
       {
          accessoryInstance.transform.parent = gameObject.transform;
          accessoryInstance.transform.localPosition = Vector3.zero;
          accessoryInstance.transform.localRotation = Quaternion.identity;
          SkinnedMeshRenderer skinnedMesh = accessoryInstance.GetComponentInChildren<SkinnedMeshRenderer>();
          if (skinnedMesh != null)
          {
             {
                skinnedMesh.sharedMaterial = new Material(skinnedMesh.material);
             }
             if (texture != null)
             {
                skinnedMesh.sharedMaterial.mainTexture = texture;
             }
             ReparentBones(skinnedMesh);
          }
          m_AvatarAccessories[nIndex] = accessoryInstance;
       }
    }

    protected void ReparentBones(SkinnedMeshRenderer accessoryRenderer)
    {
       if (accessoryRenderer == null || accessoryRenderer.bones == null)
       {
          return;
       }

       Transform[] newBones = new Transform[accessoryRenderer.bones.Length];
       Transform rootTransform = transform;

       for (int i = 0; i < accessoryRenderer.bones.Length; i++)
       {
          if (accessoryRenderer.bones[i] == null)
          {
             continue;
          }
          string boneName = accessoryRenderer.bones[i].name;
          Transform[] allBones = rootTransform.GetComponentsInChildren<Transform>();
          foreach (Transform bone in allBones)
          {
             if (bone.name == boneName)
             {
                newBones[i] = bone;
                break;
             }
          }
          if (newBones[i] == null)
          {
             Debug.LogWarning("Accessory bone not found: " + boneName);
          }
       }
       accessoryRenderer.bones = newBones;
    }

    protected void Replace(SkinnedMeshRenderer skinnedmeshrenderer_old, SkinnedMeshRenderer skinnedmeshrenderer_new)
    {
       if (skinnedmeshrenderer_old == null || skinnedmeshrenderer_new == null)
       {
          return;
       }
       Transform[] array = new Transform[skinnedmeshrenderer_new.bones.Length];
       for (int i = 0; i < skinnedmeshrenderer_new.bones.Length; i++)
       {
          Transform transform = null;
          Transform[] componentsInChildren = skinnedmeshrenderer_old.transform.parent.GetComponentsInChildren<Transform>();
          foreach (Transform transform2 in componentsInChildren)
          {
             if (transform2.name == skinnedmeshrenderer_new.bones[i].name)
             {
                transform = transform2;
             }
          }
          if (transform == null)
          {
             Debug.LogWarning(skinnedmeshrenderer_new.bones[i].name + " be not finded in player bones");
             break;
          }
          array[i] = transform;
       }
       skinnedmeshrenderer_old.bones = array;
       skinnedmeshrenderer_old.sharedMesh = skinnedmeshrenderer_new.sharedMesh;
    }
}