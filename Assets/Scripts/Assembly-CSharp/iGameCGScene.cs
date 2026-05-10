using UnityEngine;
using UnityEngine.SceneManagement;

public class iGameCGScene : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadSceneAsync("Scene_Main");
    }

    private void Update()
    {
    }
}