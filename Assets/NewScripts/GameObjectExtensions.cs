using UnityEngine;

public static class GameObjectExtensions
{
    public static void SetActiveRecursive(this GameObject obj, bool isActive)
    {
        if (obj == null) return;
        Transform[] children = obj.GetComponentsInChildren<Transform>(true);
        if (children.Length <= 1 && obj.activeSelf == isActive)
        {
            return;
        }

        bool allMatch = true;
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].gameObject.activeSelf != isActive)
            {
                allMatch = false;
                break;
            }
        }
        
        if (allMatch) return;
        for (int i = 0; i < children.Length; i++)
        {
            children[i].gameObject.SetActive(isActive);
        }
    }
}