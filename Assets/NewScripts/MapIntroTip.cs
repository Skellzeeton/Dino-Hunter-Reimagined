using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapIntroTip : MonoBehaviour
{
    private string currentScene = "";
    private bool hasShownTip = false;
    private iGameSceneBase m_GameScene;

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
        hasShownTip = false;

        // Start a coroutine to wait for the actual gameplay phase
        StartCoroutine(WaitForGameToActuallyStart());
    }

    private IEnumerator WaitForGameToActuallyStart()
    {
        while (iGameApp.GetInstance() == null || iGameApp.GetInstance().m_GameScene == null)
            yield return null;
        
        m_GameScene = iGameApp.GetInstance().m_GameScene;
        while (m_GameScene.GameStatus != iGameSceneBase.kGameStatus.GameBegin)
            yield return null;
        
        yield return new WaitForSeconds(1.5f);
        iGameUIBase gameUI = m_GameScene.GetGameUI();
        if (gameUI != null && !hasShownTip)
        {
            string tipMessage = GetMapIntroMessage(SceneManager.GetActiveScene().name);
            if (!string.IsNullOrEmpty(tipMessage))
            {
                gameUI.ShowTip(tipMessage);
                hasShownTip = true;
            }
        }
    }

    private string GetMapIntroMessage(string sceneName)
    {
        if (IsSceneOfType(1, sceneName))
            return "The Lost Jungle";
        if (IsSceneOfType(2, sceneName))
            return "The Forsaken Woods";
        if (IsSceneOfType(3, sceneName))
            return "Boiling Depths";
        if (IsSceneOfType(4, sceneName))
            return "Scorched Abyss";
        if (IsSceneOfType(5, sceneName))
            return "The Gorge";
        if (IsSceneOfType(6, sceneName))
            return "The Death Valley";
        if (IsSceneOfType(7, sceneName))
            return "Ancient Snowy Heights";
        if (IsSceneOfType(8, sceneName))
            return "The Glacial Earthscape";
        if (IsSceneOfType(9, sceneName))
            return "Scorching Badlands";
        return "";
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
                       sceneName == "SceneForest_Rainy";
            case 2:
                return sceneName.StartsWith("Yulin_");
            case 3:
                return sceneName == "SceneLava";
            case 4:
                return sceneName == "SceneLava2";
            case 5:
                return sceneName == "SceneGorge" ||
                       sceneName == "SceneGorge_Dusk";
            case 6:
                return sceneName == "SceneGorge_DeathValley";
            case 7:
                return sceneName == "SceneSnow";
            case 8:
                return sceneName == "SceneIce";
            case 9:
                return sceneName == "SceneScorch";
            default:
                return false;
        }
    }
}
