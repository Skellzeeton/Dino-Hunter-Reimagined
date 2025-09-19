/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MobPrefabEntry
{
    public int      mobID;
    public GameObject prefab;
}

public class MobPreloaderRuntime : MonoBehaviour
{
    [Header("Assign mobID → prefab in the Inspector")]
    [SerializeField] private MobPrefabEntry[] mobPrefabs;

    private Dictionary<int, GameObject>    mobIdToPrefab = new Dictionary<int, GameObject>();
    private Dictionary<string, GameObject> mobPreloaded  = new Dictionary<string, GameObject>();
    private Dictionary<string, bool>       mobHasSpawned = new Dictionary<string, bool>();

    private Transform mPreLoadNode;
    private bool      hasPreloaded = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Create or find the hidden root for our preloads
        GameObject root = GameObject.Find("MobPreLoad");
        if (root == null)
            root = new GameObject("MobPreLoad");
        mPreLoadNode = root.transform;

        // Build lookup from mobID → prefab
        for (int i = 0; i < mobPrefabs.Length; i++)
        {
            MobPrefabEntry entry = mobPrefabs[i];
            if (entry != null && entry.prefab != null)
            {
                mobIdToPrefab[entry.mobID] = entry.prefab;
                Debug.Log("Mapped mobID " + entry.mobID + " → " + entry.prefab.name);
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Fired whenever a new scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only preload once, on the first qualifying scene
        if (!hasPreloaded)
        {
            // Replace this with however you get the current levelID
            iGameState gameState = iGameApp.GetInstance().m_GameState;
            if (gameState != null && gameState.GameLevel > 0)
            {
                PreloadMobsFromLevel(gameState.GameLevel);
                hasPreloaded = true;
            }
        }
    }

    private void PreloadMobsFromLevel(int levelID)
    {
        TextAsset lvlXml  = Resources.Load("spoofedconfigs/gamelevel") as TextAsset;
        TextAsset waveXml = Resources.Load("spoofedconfigs/gamewave")  as TextAsset;
        if (lvlXml == null || waveXml == null)
        {
            return;
        }

        List<string> waveIDs = GetGameWaveIDs(lvlXml.text, levelID);
        if (waveIDs.Count == 0)
        {
            return;
        }

        List<int> allMobIDs = new List<int>();
        for (int i = 0; i < waveIDs.Count; i++)
        {
            string wid = waveIDs[i];
            List<int> mids = GetMobIDsFromWave(waveXml.text, wid);

            for (int j = 0; j < mids.Count; j++)
            {
                int mid = mids[j];
                if (!allMobIDs.Contains(mid))
                    allMobIDs.Add(mid);
            }
        }

        string[] idStrings = new string[allMobIDs.Count];
        for (int i = 0; i < allMobIDs.Count; i++)
            idStrings[i] = allMobIDs[i].ToString();
        Debug.Log("Mob IDs for level " + levelID + ": " + string.Join(", ", idStrings));
        PreloadMobsFromIDs(allMobIDs);
    }

    private List<string> GetGameWaveIDs(string xmlText, int levelID)
    {
        List<string> waveIDs = new List<string>();
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlText);
        XmlNode levelNode = doc.SelectSingleNode(
            "//root/gamelevel[@id='" + levelID + "']");
        if (levelNode != null && levelNode.Attributes["gamewave"] != null)
        {
            string raw = levelNode.Attributes["gamewave"].Value;
            string[] parts = raw.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string w = parts[i].Trim();
                if (w.Length > 0 && !waveIDs.Contains(w))
                    waveIDs.Add(w);
            }
        }
        return waveIDs;
    }

    private List<int> GetMobIDsFromWave(string xmlText, string waveID)
    {
        List<int> mobIDs = new List<int>();
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlText);

        XmlNodeList nodes = doc.SelectNodes(
            "//root/gamewave[@id='" + waveID + "']/mob");
        for (int i = 0; i < nodes.Count; i++)
        {
            XmlNode node = nodes[i];
            if (node.Attributes != null && node.Attributes["mob_id"] != null)
            {
                string sid = node.Attributes["mob_id"].Value;
                int parsed;
                if (int.TryParse(sid, out parsed) && !mobIDs.Contains(parsed))
                {
                    mobIDs.Add(parsed);
                }
            }
        }
        return mobIDs;
    }

    private void PreloadMobsFromIDs(List<int> mobIDs)
    {
        for (int i = 0; i < mobIDs.Count; i++)
        {
            int id = mobIDs[i];
            if (!mobIdToPrefab.ContainsKey(id))
            {
                Debug.LogWarning("No prefab mapped for mobID " + id);
                continue;
            }

            GameObject prefab = mobIdToPrefab[id];
            string key = prefab.name;
            if (mobPreloaded.ContainsKey(key))
                continue;

            GameObject inst = Instantiate(prefab, mPreLoadNode);
            inst.name = key;
            inst.SetActive(true);

            mobPreloaded[key]  = inst;
            mobHasSpawned[key] = false;
            Debug.Log("✅ Preloaded mob: " + key);
        }
    }

    private void Update()
    {
        List<string> keys = new List<string>();
        foreach (string k in mobPreloaded.Keys)
            keys.Add(k);

        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            if (!mobHasSpawned.ContainsKey(key) || mobHasSpawned[key])
                continue;

            if (GameObject.Find(key) != null)
            {
                mobHasSpawned[key] = true;
                StartCoroutine(RemoveAfterDelay(key, 30f));
            }
        }
    }

    private IEnumerator RemoveAfterDelay(string key, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (GameObject.Find(key) == null &&
            mobPreloaded.ContainsKey(key))
        {
            Destroy(mobPreloaded[key]);
            mobPreloaded.Remove(key);
            mobHasSpawned.Remove(key);
            Debug.Log("🧹 Removed unused preloaded mob: " + key);
        }
    }
}*/
