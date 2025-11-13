using UnityEngine;

public class IngameUI : MonoBehaviour
{
    public GameObject GameUIPrefab;

    private GameObject currentGameUI;

    private void LoadcurrentGameUI()
    {
        if (currentGameUI != null || GameUIPrefab == null) return;

        currentGameUI = Instantiate(GameUIPrefab);
    }
}
