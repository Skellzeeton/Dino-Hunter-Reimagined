using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class iGameCGAdvert : MonoBehaviour
{

    [Header("Splash Screen")]

    public Material splashMaterial;

    public Texture2D splashTexture;

    public float splashDuration = 3f;
    private GameObject canvasGO;
    private RawImage rawImage;
    private CanvasGroup canvasGroup;
    private const float fadeTime = 0.35f;
    
    public class CServerAdvertInfo
    {
        public string sVideo = string.Empty;
        public string sVideoUrl = string.Empty;
        public Dictionary<int, string> dictAdvertUrl = new Dictionary<int, string>();

        public void LoadData(XmlDocument doc)
        {
        }
    }

    protected string m_sUrl = iMacroDefine.CompanyAccountURL;
    protected string m_sUrl_File = "CoMDH_AdvertConfig";
    public string m_sServerInfoKey = "trinitigame_comdh";

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        StartCoroutine(SplashAndLoad());
    }

    private IEnumerator SplashAndLoad()
    { 
        if (splashMaterial != null || splashTexture != null)
            CreateSplash();
        if (canvasGroup != null)
            yield return StartCoroutine(FadeCanvas(0f, 1f, fadeTime));
        if (splashDuration > 0f)
            yield return new WaitForSeconds(splashDuration);
        if (canvasGroup != null)
            yield return StartCoroutine(FadeCanvas(1f, 0f, fadeTime));
        if (canvasGO != null)
            Destroy(canvasGO);
        VisitServerConfig();
    }

    private void VisitServerConfig()
    {
        SceneManager.LoadSceneAsync("Scene_Main");
    }
    
    private void CreateSplash()
    {
        canvasGO = new GameObject("SplashCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.transform.localScale = Vector3.one;
        canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        GameObject imgGO = new GameObject("SplashImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        rawImage = imgGO.AddComponent<RawImage>();
        Texture mainTex = null;
        if (splashMaterial != null)
        {
            rawImage.material = splashMaterial;
            mainTex = splashMaterial.mainTexture;
            if (mainTex != null)
            {
                rawImage.texture = mainTex;
            }
        }

        if (mainTex == null && splashTexture != null)
        {
            rawImage.texture = splashTexture;
            mainTex = splashTexture;
        }
        RectTransform rt = rawImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        FitToScreen(rt, mainTex);
        
        rt.localScale = new Vector3(0.45f, 0.5f, 1f);
    }

    private void FitToScreen(RectTransform rt, Texture tex)
    {
        if (tex != null)
        {
            float imgW = tex.width;
            float imgH = tex.height;
            if (imgW <= 0f || imgH <= 0f)
            {
                rt.sizeDelta = new Vector2(Screen.width, Screen.height);
                rt.anchoredPosition = Vector2.zero;
                return;
            }

            float scale = Mathf.Max((float)Screen.width / imgW, (float)Screen.height / imgH);
            rt.sizeDelta = new Vector2(imgW * scale, imgH * scale);
            rt.anchoredPosition = Vector2.zero;
            return;
        }
        rt.sizeDelta = new Vector2(Screen.width, Screen.height);
        rt.anchoredPosition = Vector2.zero;
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
