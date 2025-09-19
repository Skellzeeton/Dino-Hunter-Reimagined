using UnityEngine;

public class SceneMain_Controller : MonoBehaviour
{
   // public GameObject videoPrefab;
   
    public GameObject mainPrefab;

    private GameObject currentVideoInstance;
    private GameObject UIPfb;

    /*private float tapTimer = 0f;
    private int tapCount = 0;
    private bool videoPlaying = false;*/

private void Start()
    {
        LoadMainPrefab();
    }
    
    /*private void Start()
    {
        LoadVideo();
    }

    private void Update()
    {
        HandleDoubleTap();

        if (videoPlaying && IsVideoFinished())
        {
            LoadMainPrefab();
        }
    }

    private void HandleDoubleTap()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tapCount++;
            if (tapCount == 1)
            {
                tapTimer = Time.time;
            }
            else if (tapCount == 2 && Time.time - tapTimer <= 0.5f)
            {
                SkipVideo();
            }
        }

        if (tapCount > 0 && Time.time - tapTimer > 0.5f)
        {
            tapCount = 0;
        }
    }

    private void LoadVideo()
    {
        if (videoPrefab != null)
        {
            currentVideoInstance = Instantiate(videoPrefab);
            videoPlaying = true;
            CUISound.GetInstance().Play("DinoHunterREIMAGINED");
        }
    }*/

    private void LoadMainPrefab()
    {
        if (UIPfb != null || mainPrefab == null) return;

        UIPfb = Instantiate(mainPrefab);

        //DisableClicksTemporarily(currentMainInstance);
        
        /*if (currentVideoInstance != null)
        {
            Destroy(currentVideoInstance);
            currentVideoInstance = null;
            videoPlaying = false; ;
            CUISound.GetInstance().Stop("DinoHunterREIMAGINED");
        }
    }

    private bool IsVideoFinished()
    {
        return Time.timeSinceLevelLoad > 23f;
    }

    private void SkipVideo()
    {
        Debug.Log("⚡ Video skipped by double-tap.");
        LoadMainPrefab();
    }

    private void DisableClicksTemporarily(GameObject target)
    {
        MonoBehaviour[] scripts = target.GetComponentsInChildren<MonoBehaviour>(true);
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);

        foreach (var script in scripts) script.enabled = false;
        foreach (var col in colliders) col.enabled = false;

        StartCoroutine(ReenableInteractionAfterDelay(scripts, colliders, 1.5f));
    }

    private System.Collections.IEnumerator ReenableInteractionAfterDelay(MonoBehaviour[] scripts, Collider[] colliders, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var script in scripts) script.enabled = true;
        foreach (var col in colliders) col.enabled = true;*/
    }
}
