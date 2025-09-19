using UnityEngine;
using UnityEngine.SceneManagement;

public class Limiter : MonoBehaviour
{
    private static Limiter instance;

    void Awake()
    {
        if (!Application.isPlaying) return;

        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void OnEnable()
    {
        if (!Application.isPlaying) return;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (!Application.isPlaying) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "SceneLoad" || currentScene == "Scene_MainMenu")
        {
            CallClampAndFix();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SceneLoad" || scene.name == "Scene_MainMenu")
        {
            CallClampAndFix();
        }
    }

    void CallClampAndFix()
    {
        iDataCenter data = iGameApp.GetInstance().m_GameData.m_DataCenter;
        if (data != null)
        {
            data.ClampToLimits();
        }
        else
        {
            Debug.LogWarning("DataCenter is null!");
        }
    }
}