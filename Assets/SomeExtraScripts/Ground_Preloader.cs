using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Ground_Preloader : MonoBehaviour
{
    [System.Serializable]
    public class PrefabEntry
    {
        public GameObject prefab;
        public int type;
    }

    public List<PrefabEntry> prefabsToPreload = new List<PrefabEntry>();

    private List<GameObject> tempInstances = new List<GameObject>();
    private string currentScene = "";
    private bool hasPreloaded = false;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == currentScene)
            return;

        currentScene = scene.name;

        if (IsSceneInAnyType(currentScene))
        {
            if (!hasPreloaded)
            {
                StartCoroutine(PreloadPrefabs());
                hasPreloaded = true;
            }
        }
        else
        {
            UnloadPrefabs();
            hasPreloaded = false;
        }
    }

    private bool IsSceneInAnyType(string sceneName)
    {
        for (int type = 1; type <= 10; type++)
        {
            if (IsSceneOfType(type, sceneName))
                return true;
        }
        return false;
    }

    private bool IsSceneOfType(int type, string sceneName)
    {
        switch (type)
        {
            case 1:
                return sceneName == "SceneForest" ||
                       sceneName == "SceneForest_Boss" ||
                       sceneName == "SceneForest_Dusk" || 
                       sceneName == "SceneForest_Night" || 
                       sceneName == "Yulin_shaguai01" ||
                       sceneName == "Yulin_shaguai02" || sceneName == "Yulin_shaguai03" ||
                       sceneName == "Yulin_shaguai04" || sceneName == "Yulin_toudan01" ||
                       sceneName == "Yulin_toudan02" || sceneName == "Yulin_toudan03" ||
                       sceneName == "Yulin_toudan04" || sceneName == "Yulin_toudan05" ||
                       sceneName == "Yulin_shouwei01" || sceneName == "Yulin_shouwei01_dusk" ||
                       sceneName == "Yulin_shouwei02" || sceneName == "Yulin_shouwei03" ||
                       sceneName == "Yulin_shouwei04" || sceneName == "Yulin_shouwei05" ||
                       sceneName == "Yulin_shouwei06" || sceneName == "Yulin_shaguai02_night" ||
                       sceneName == "SceneScorch" || sceneName == "Yulin_shouwei07" ||
                       sceneName == "SceneIce" || sceneName == "SceneLava" ||
                       sceneName == "Yulin_ultimate01" || sceneName == "SceneLava2" ||
                       sceneName == "SceneLava3" || sceneName == "SceneSnow";

            case 2:
                return sceneName == "SceneGorge" || sceneName == "SceneGorge_DeathValley";

            case 3:
                return sceneName == "SceneForest_Rainy" || sceneName == "SceneForest_Boss";
            
            case 4:
                return sceneName == "SceneLava" || sceneName == "SceneLava2" ||
                       sceneName == "SceneLava3";

            default:
                return false;
        }
    }



    private IEnumerator PreloadPrefabs()
    {
        for (int i = 0; i < prefabsToPreload.Count; i++)
        {
            PrefabEntry entry = prefabsToPreload[i];
            if (entry == null || entry.prefab == null)
                continue;

            if (ShouldLoadForScene(entry.type, currentScene))
            {
                GameObject inst = Instantiate(entry.prefab);
                inst.transform.SetParent(null);
                inst.SetActive(true);

                Animator animator = inst.GetComponentInChildren<Animator>();
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
                    animator.Update(0f);
                }

                Animation legacy = inst.GetComponentInChildren<Animation>();
                if (legacy != null)
                {
                    foreach (AnimationState state in legacy)
                    {
                        legacy.Play(state.name);
                        legacy.Sample();
                        legacy.Stop();
                    }
                }

                inst.SetActive(false);
                tempInstances.Add(inst);
            }
        }

        yield break;
    }

    private bool ShouldLoadForScene(int type, string sceneName)
    {
        if (type == 0)
            return true;

        return IsSceneOfType(type, sceneName);
    }

    private void UnloadPrefabs()
    {
        for (int i = 0; i < tempInstances.Count; i++)
        {
            if (tempInstances[i] != null)
                Destroy(tempInstances[i]);
        }
        tempInstances.Clear();
    }
}
