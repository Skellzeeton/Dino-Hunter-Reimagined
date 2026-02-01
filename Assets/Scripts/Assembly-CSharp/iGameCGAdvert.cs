using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

public class iGameCGAdvert : MonoBehaviour
{
    public class CServerAdvertInfo
    {
        public string sVideo = string.Empty;
        public string sVideoUrl = string.Empty;
        public Dictionary<int, string> dictAdvertUrl;

        public CServerAdvertInfo()
        {
            dictAdvertUrl = new Dictionary<int, string>();
        }

        public void LoadData(XmlDocument doc)
        {
        }

        protected bool GetAttribute(XmlNode node, string name, ref string value)
        {
            // Stubbed: Always return false
            return false;
        }
    }

    protected string m_sUrl = iMacroDefine.CompanyAccountURL;
    protected string m_sUrl_File = "CoMDH_AdvertConfig";
    protected string m_sServerInfoFilePathSrc = "D:\\development\\_DinoCapWorld\\src\\_DinoCapWorld\\Documents\\serverconfig";
    protected string m_sServerInfoFilePathDst = "D:\\development\\_DinoCapWorld\\src\\_DinoCapWorld\\Documents\\serverconfig";
    public string m_sServerInfoKey = "trinitigame_comdh";

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        SceneManager.LoadSceneAsync("Scene_Main");
    }

    private void Update()
    {
    }

    protected void OnResult(string filename, string url = "")
    {
        SceneManager.LoadSceneAsync("Scene_Main");
    }

    protected void OnSuccess(string sFileData)
    {
        OnResult(string.Empty, string.Empty);
    }

    protected void OnFailed()
    {
        OnResult(string.Empty, string.Empty);
    }

    protected string TransformXML2TXT(string srcpath, string dstpath, string key)
    {
        return string.Empty;
    }
}