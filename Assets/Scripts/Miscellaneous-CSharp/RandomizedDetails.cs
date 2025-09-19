/*using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class RandomizedDetails :
MonoBehaviour
{
    private readonly List<string> forcedExtras = new List<string> { "Home_biaozhi01", "level2_dimian2" };
    private string keyA = "level2_shan02";
    private string keyB = "Home_shu03";
    private int totalParts = 18;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "SceneForest") return;

        Transform detailsRoot = FindInScene();
        if (detailsRoot == null) return;

        detailsRoot.gameObject.SetActive(true);

        List<Transform> allParts = new List<Transform>();
        foreach (Transform child in detailsRoot)
        {
            allParts.Add(child);
        }

        List<Transform> selected = new List<Transform>();
        bool homeSelected = false;

        // Force one key detail
        Transform keyDetail = Random.value < 0.5f
            ? allParts.FirstOrDefault(t => t.name == keyA)
            : allParts.FirstOrDefault(t => t.name == keyB);

        if (keyDetail != null)
        {
            selected.Add(keyDetail);
            homeSelected = keyDetail.name == keyB;
        }

        // If Home_shu03 is chosen, add its required extras
        if (homeSelected)
        {
            foreach (string extraName in forcedExtras)
            {
                Transform extra = allParts.FirstOrDefault(t => t.name == extraName);
                if (extra != null && !selected.Contains(extra))
                {
                    selected.Add(extra);
                }
            }
        }

        // Remove any parts we can’t load if Home_shu03 is NOT selected
        List<Transform> candidates = allParts
            .Where(t =>
                !selected.Contains(t) &&
                (!forcedExtras.Contains(t.name) || homeSelected))
            .OrderBy(t => Random.value)
            .ToList();

        foreach (Transform part in candidates)
        {
            if (selected.Count >= totalParts) break;
            selected.Add(part);
        }

        // Toggle visibility
        foreach (Transform child in allParts)
        {
            child.gameObject.SetActive(selected.Contains(child));
        }

        Debug.Log("[RandomForest] Loaded " + selected.Count + " parts. Home_shu03 selected: " + homeSelected);
    }

    private Transform FindInScene()
    {
        // Assumes hierarchy: DinoCapWorld2 -> DinoCapWorld2 -> RandomForestDetails
        GameObject outer = GameObject.Find("DinoCapWorld2");
        if (outer == null) return null;

        Transform inner = outer.transform.Find("DinoCapWorld2");
        if (inner == null) return null;

        Transform details = inner.Find("RandomForestDetails");
        return details;
    }
}*/
