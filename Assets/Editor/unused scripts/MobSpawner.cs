using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class MobPrefabEntry
{
    public int mobID;
    public GameObject prefab;
}

public class MobSpawner : MonoBehaviour
{
    [Header("Assign mobID → prefab in the Inspector")]
    [SerializeField] private MobPrefabEntry[] mobPrefabs;

    private Dictionary<int, GameObject> mobIdToPrefab = new Dictionary<int, GameObject>();
    private Dictionary<string, GameObject> activeMobs = new Dictionary<string, GameObject>();
    private Dictionary<string, bool> hasSpawned = new Dictionary<string, bool>();

    private Transform spawnRoot;

    private void Awake()
    {
        GameObject root = GameObject.Find("MobSpawnRoot");
        if (root == null)
            root = new GameObject("MobSpawnRoot");
        spawnRoot = root.transform;

        for (int i = 0; i < mobPrefabs.Length; i++)
        {
            MobPrefabEntry entry = mobPrefabs[i];
            if (entry != null && entry.prefab != null)
            {
                mobIdToPrefab[entry.mobID] = entry.prefab;
            }
        }
    }

    public void SpawnMobsForLevel(int levelID)
    {
        TextAsset lvlXml = Resources.Load("spoofedconfigs/gamelevel") as TextAsset;
        TextAsset waveXml = Resources.Load("spoofedconfigs/gamewave") as TextAsset;
        if (lvlXml == null || waveXml == null)
        {
            Debug.LogWarning("Missing XML config files.");
            return;
        }

        List<string> waveIDs = GetGameWaveIDs(lvlXml.text, levelID);
        List<int> mobIDs = new List<int>();

        for (int i = 0; i < waveIDs.Count; i++)
        {
            List<int> ids = GetMobIDsFromWave(waveXml.text, waveIDs[i]);
            for (int j = 0; j < ids.Count; j++)
            {
                int id = ids[j];
                if (!mobIDs.Contains(id))
                    mobIDs.Add(id);
            }
        }

        for (int i = 0; i < mobIDs.Count; i++)
        {
            int mobID = mobIDs[i];
            if (!mobIdToPrefab.ContainsKey(mobID))
            {
                Debug.LogWarning("No prefab mapped for mobID " + mobID);
                continue;
            }

            GameObject prefab = mobIdToPrefab[mobID];
            string key = prefab.name + "_" + mobID;

            if (activeMobs.ContainsKey(key))
                continue;

            GameObject inst = Instantiate(prefab, spawnRoot);
            inst.name = key;
            inst.SetActive(true);

            activeMobs[key] = inst;
            hasSpawned[key] = false;

            Debug.Log("✅ Spawned mob: " + key);
        }
    }

    private List<string> GetGameWaveIDs(string xmlText, int levelID)
    {
        List<string> waveIDs = new List<string>();
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlText);
        XmlNode levelNode = doc.SelectSingleNode("//root/gamelevel[@id='" + levelID + "']");
        if (levelNode != null && levelNode.Attributes["gamewave"] != null)
        {
            string[] parts = levelNode.Attributes["gamewave"].Value.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string trimmed = parts[i].Trim();
                if (trimmed.Length > 0 && !waveIDs.Contains(trimmed))
                    waveIDs.Add(trimmed);
            }
        }
        return waveIDs;
    }

    private List<int> GetMobIDsFromWave(string xmlText, string waveID)
    {
        List<int> mobIDs = new List<int>();
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlText);
        XmlNodeList nodes = doc.SelectNodes("//root/gamewave[@id='" + waveID + "']/mob");
        for (int i = 0; i < nodes.Count; i++)
        {
            XmlNode node = nodes[i];
            if (node.Attributes != null && node.Attributes["mob_id"] != null)
            {
                string sid = node.Attributes["mob_id"].Value;
                int parsed = 0;
                if (int.TryParse(sid, out parsed) && !mobIDs.Contains(parsed))
                    mobIDs.Add(parsed);
            }
        }
        return mobIDs;
    }

    private void Update()
    {
        List<string> keys = new List<string>();
        foreach (string key in activeMobs.Keys)
            keys.Add(key);

        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            if (!hasSpawned.ContainsKey(key) || hasSpawned[key])
                continue;

            if (GameObject.Find(key) != null)
            {
                hasSpawned[key] = true;
                StartCoroutine(RemoveAfterDelay(key, 30f));
            }
        }
    }

    private IEnumerator RemoveAfterDelay(string key, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (GameObject.Find(key) == null && activeMobs.ContainsKey(key))
        {
            Destroy(activeMobs[key]);
            activeMobs.Remove(key);
            hasSpawned.Remove(key);
            Debug.Log("🧹 Removed unused mob: " + key);
        }
    }
}
