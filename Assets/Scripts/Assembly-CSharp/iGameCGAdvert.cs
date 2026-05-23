using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

public class iGameCGAdvert : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        SceneManager.LoadSceneAsync("Scene_Main");
    }
}