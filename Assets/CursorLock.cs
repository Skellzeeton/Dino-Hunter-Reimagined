using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorLock : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool isPaused = false;
    private bool isManuallyUnlocked = false;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("Save path: " + Application.persistentDataPath);
        
        if (IsSceneThatRequiresCursorLock(SceneManager.GetActiveScene().name))
            LockCursor();
        else
            UnlockCursor();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (Input.GetKeyDown(KeyCode.F1))
            ToggleCursorLock();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isPaused && !isManuallyUnlocked && IsSceneThatRequiresCursorLock(scene.name))
            LockCursor();
        else
            UnlockCursor();
    }

       private bool IsSceneThatRequiresCursorLock(string sceneName)
              {
                  return sceneName == "SceneForest" || sceneName == "SceneGorge" ||
                         sceneName == "SceneForest_Dusk" || sceneName == "SceneGorge 1" ||
                         sceneName == "SceneForest_Night" || sceneName == "SceneGorge 2" ||
                      sceneName == "SceneForest_Rainy" || sceneName == "Yulin_shaguai01" ||
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
                      sceneName == "SceneLava2" || sceneName == "SceneSnow";
              }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);

        if (isPaused)
        {
            UnlockCursor();
        }
        else
        {
            if (IsSceneThatRequiresCursorLock(SceneManager.GetActiveScene().name))
            {
                LockCursor();
                isManuallyUnlocked = false;
            }
        }
    }

    private void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            UnlockCursor();
            isManuallyUnlocked = true;
        }
        else
        {
            LockCursor();
            isManuallyUnlocked = false;
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}