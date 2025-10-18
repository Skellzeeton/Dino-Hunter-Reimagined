using UnityEngine;

public class LegacyAnimationPreloader : MonoBehaviour
{
    [Header("Assign your legacy-animated prefab here")]
    public GameObject prefabToPreload;

    private static GameObject persistentInstance;

    void Awake()
    {
        if (persistentInstance != null)
        {
            Destroy(gameObject); // Already loaded
            return;
        }

        if (prefabToPreload == null)
        {
            Debug.LogWarning("LegacyAnimationPreloader: No prefab assigned.");
            Destroy(gameObject);
            return;
        }

        // Instantiate and hide
        persistentInstance = Instantiate(prefabToPreload);
        persistentInstance.name = "[Preloaded]_" + prefabToPreload.name;
        persistentInstance.SetActive(false);
        DontDestroyOnLoad(persistentInstance);

        // Preload legacy animations
        Animation anim = persistentInstance.GetComponent<Animation>();
        if (anim != null)
        {
            foreach (AnimationState state in anim)
            {
                if (state.clip != null)
                {
                    state.clip.SampleAnimation(persistentInstance, 0f); // Preload each clip
                }
            }
        }
    }
}