﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GameAnalyticsSDK;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class ResolutionManager : Singleton<ResolutionManager>
{
    [Header("Fullscreen")]
    public bool fullScreen = true;

    [Header("Scroll View")]
    public MenuScrollRect resolutionScrollRect;
    public GameObject resolutionContentParent;
    public ToggleGroup resolutionsToggleGroup;
    public float initialYPos = -36f;
    public float gapHeight = 110f;

    [Header("Resolutions")]
    public Text currentResText;
    public GameObject resolutionLinePrefab;
    public List<Vector2> allScreenRes;

    [Header("Ratio")]
    public List<Vector2> allRatios;

    [Header("4:3")]
    public List<Vector2> screenRes1;
    [Header("5:4")]
    public List<Vector2> screenRes2;
    [Header("16:10")]
    public List<Vector2> screenRes3;
    [Header("16:09")]
    public List<Vector2> screenRes4;

    [Header("Settings")]
    public Vector2 currentScreenRes = new Vector2();
    public Vector2 currentDynamicScreenRes = new Vector2();

    [Header("Toggles")]
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;

    public List<GameObject> allToggles = new List<GameObject>();

    [Header("Framerate")]
    public float frameRate;

    [Header("Dynamic Resolution")]
    public bool dynamicResolution = true;
    public bool dynamicResolutionDebug = false;
    public float resolutionFactor = 1;
    public float frameRateTarget = 200f;
    public float frameRateTargetVSync = 60f;
    public float frameRateGap = 10f;

    [Header("Dynamic Resolution Advanced")]
    public float frameRateSamplesCount = 10;
    public float frameRateOffset = 0.05f;
    public float frameRateSamplesTime = 0.2f;
    public float frameRatePause = 2f;

    private List<float> frameRateSamples = new List<float>();
    private bool isAnalysingFrameRate = false;

    private bool selectingToggle = false;

    void Start()
    {
        LoadData();

        StartCoroutine(CheckFullScreenChange(Screen.fullScreen));

        CreateResolutionLines();

        if (!PlayerPrefs.HasKey("ScreenWidth"))
            FindResolution();

        SaveData();

        if (dynamicResolution)
            StartCoroutine(AnalyseFramerate());
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            fullScreen = PlayerPrefs.GetInt("Fullscreen") == 1 ? true : false;
            fullscreenToggle.isOn = fullScreen;
        }

        if (PlayerPrefs.HasKey("Vsync"))
        {
            QualitySettings.vSyncCount = PlayerPrefs.GetInt("Vsync");
            vsyncToggle.isOn = PlayerPrefs.GetInt("Vsync") == 0 ? false : true;
        }

        if (PlayerPrefs.HasKey("ScreenWidth"))
        {
            SetResolution(new Vector2(PlayerPrefs.GetInt("ScreenWidth"), PlayerPrefs.GetInt("ScreenHeight")));
            Debug.Log("Resolution Loaded : " + currentScreenRes.x + "x" + currentScreenRes.y);
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("ScreenWidth", (int)currentScreenRes.x);
        PlayerPrefs.SetInt("ScreenHeight", (int)currentScreenRes.y);

        PlayerPrefs.SetInt("Fullscreen", fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("Vsync", QualitySettings.vSyncCount);
    }

    void CreateResolutionLines()
    {
        allScreenRes.AddRange(screenRes1);
        allScreenRes.AddRange(screenRes2);
        allScreenRes.AddRange(screenRes3);
        allScreenRes.AddRange(screenRes4);

        allToggles.Clear();

        List<Vector2> allResTemp = new List<Vector2>(allScreenRes);
        int resCount = 0;

        //Scroll Rect Height
        resolutionContentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(resolutionContentParent.GetComponent<RectTransform>().sizeDelta.x, (50 + gapHeight * allScreenRes.Count) * resolutionScrollRect.heightFactor);

        while (allResTemp.Count != 0)
        {
            Vector2 smallestRes = allResTemp[0];

            foreach (Vector2 v in allResTemp)
            {
                if (v.y < smallestRes.y)
                    smallestRes = v;
            }

            allResTemp.Remove(smallestRes);

            Vector3 pos = Vector3.zero;
            pos.y = -gapHeight * resCount + initialYPos;
            pos.z = 0;

            GameObject resLine = Instantiate(resolutionLinePrefab, resolutionLinePrefab.transform.position, resolutionLinePrefab.transform.rotation, resolutionContentParent.transform) as GameObject;

            allToggles.Add(resLine);

            resLine.GetComponent<Toggle>().group = resolutionsToggleGroup;

            resLine.GetComponent<Toggle>().onValueChanged.AddListener((bool arg0) =>
                { 
                    if (arg0 && !selectingToggle)
                    {
                        SetResolution(new Vector2(smallestRes.x, smallestRes.y));
                        GraphicsQualityManager.Instance.EnableApplyButton();
                    }
                });


            //Add EventTrigger
            RectTransform resLineRect = resLine.GetComponent<RectTransform>();
            resolutionScrollRect.elements.Add(resCount, resLineRect);
            GlobalMethods.Instance.AddEventTriggerEntry(resLine, EventTriggerType.Select, () => resolutionScrollRect.CenterButton(resLineRect));

            resLine.GetComponent<RectTransform>().anchoredPosition3D = pos;
            resLine.transform.GetChild(1).GetComponent<Text>().text = smallestRes.x + "x" + smallestRes.y;
            resLine.transform.GetChild(2).GetComponent<Text>().text = FindRatio(smallestRes);

            resCount++;
        }
    }

    void Update()
    {
        if (Time.timeScale == 1)
        {
            frameRate = 1f / Time.smoothDeltaTime;

            /*if (dynamicResolution && !isAnalysingFrameRate)
                ActivateFramerateAnalyser();*/
        }
        else
        {
            frameRate = -1f;
        }
    }

    void FindResolution()
    {
        bool resolutionFound = false;
        Vector2 nativeResolution = new Vector2(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height);

        foreach (var r in  Screen.resolutions)
        {
            if (r.height > nativeResolution.y)
                nativeResolution = new Vector2(r.width, r.height);
        }
        
        //Debug.Log(nativeResolution.x + "x" + nativeResolution.y);

        foreach (Vector2 v in allScreenRes)
        {
            if (Mathf.Approximately(nativeResolution.x, v.x) && Mathf.Approximately(nativeResolution.y, v.y))
            {
                currentScreenRes = v;
                SetResolution(v);
                resolutionFound = true;
                break;
            }
        }

        if (!resolutionFound)
        {
            currentScreenRes = new Vector2(1920, 1080);
            SetResolution(currentScreenRes);
        }

        //Debug.Log(Screen.currentResolution);
        Debug.Log("Res found : " + currentScreenRes.x + "x" + currentScreenRes.y);
    }

    void SetResolution(Vector2 res)
    {
        //Debug.Log("New Resolution : " + (int)res.x + " x " + (int)res.y);

        currentScreenRes = res;
        Screen.SetResolution((int)(currentScreenRes.x * resolutionFactor), (int)(currentScreenRes.y * resolutionFactor), fullScreen);

        currentResText.text = (int)currentScreenRes.x + " x " + (int)currentScreenRes.y;

        currentDynamicScreenRes = new Vector2(currentScreenRes.x * resolutionFactor, currentScreenRes.y * resolutionFactor);

        SelectToggle();
    }

    void SelectToggle()
    {
        selectingToggle = true;

        foreach (GameObject g in allToggles)
            g.GetComponent<Toggle>().isOn = false;

        foreach (GameObject g in allToggles)
        {
            Text res = g.transform.GetChild(1).GetComponent<Text>();

            if (res.text == currentScreenRes.x + "x" + currentScreenRes.y)
            {
                g.GetComponent<Toggle>().isOn = true;
                break;
            }
        }

        selectingToggle = false;
    }

    string FindRatio(Vector2 resolution)
    {
        float testedRatio = resolution.x / resolution.y;
        string ratioText = "";

        foreach (Vector2 ratio in allRatios)
        {
            if (Mathf.Abs(testedRatio - (ratio.x / ratio.y)) < 0.01f)
            {
                ratioText = ratio.x + ":" + ratio.y;
                break;
            }
        }

        return ratioText;
    }

    public void ToggleFullscreen(bool enable)
    {
        Debug.Log("Toggle Full: " + enable);

        fullScreen = enable;
        Screen.SetResolution((int)currentScreenRes.x, (int)currentScreenRes.y, fullScreen);

        GraphicsQualityManager.Instance.EnableApplyButton();
    }

    public void ToggleVsync(bool enable)
    {
        if (enable)
            QualitySettings.vSyncCount = 1;
        else
        {
            SteamAchievements.Instance.UnlockAchievement(AchievementID.ACH_DISABLE_VSYNC);
            QualitySettings.vSyncCount = 0;
        }

        GraphicsQualityManager.Instance.EnableApplyButton();
    }

    public void Reset()
    {
        fullScreen = true;

        QualitySettings.vSyncCount = 1;
        vsyncToggle.isOn = true;

        FindResolution();

        fullscreenToggle.isOn = true;
    }

    IEnumerator CheckFullScreenChange(bool fullscreen)
    {
        yield return new WaitUntil(() => Screen.fullScreen != fullscreen);

        if (Screen.fullScreen == true && PlayerPrefs.GetInt("Fullscreen") == 0)
            fullscreenToggle.isOn = false;

        if (Screen.fullScreen == false && PlayerPrefs.GetInt("Fullscreen") == 1)
            fullscreenToggle.isOn = true;

        StartCoroutine(CheckFullScreenChange(Screen.fullScreen));
    }


    void ActivateFramerateAnalyser()
    {
        if (!dynamicResolution || isAnalysingFrameRate)
            return;

        StopCoroutine(AnalyseFramerate());
        StartCoroutine(AnalyseFramerate());
    }

    IEnumerator AnalyseFramerate()
    {
        if (!dynamicResolution)
            yield break;

        isAnalysingFrameRate = true;

        frameRateSamples.Clear();

        while (frameRateSamples.Count < frameRateSamplesCount)
        {
            yield return new WaitWhile(() => Time.timeScale != 1);

            frameRateSamples.Add(frameRate);
            yield return new WaitForSecondsRealtime(frameRateSamplesTime);
        }

        if (!Application.isFocused)
            yield return new WaitWhile(() => !Application.isFocused);

        float meanFramerate = 0;

        foreach (var f in frameRateSamples)
            meanFramerate += f;

        meanFramerate /= frameRateSamples.Count;

        float targetFrameRate = QualitySettings.vSyncCount == 0 ? frameRateTarget : frameRateTargetVSync;

        //INFERIOR
        if (meanFramerate < targetFrameRate - frameRateGap)
        {
            if (resolutionFactor > 0.5f)
            {
                if (dynamicResolutionDebug)
                    Debug.Log(meanFramerate + " is < " + (targetFrameRate - frameRateGap).ToString());
                
                resolutionFactor -= frameRateOffset;

                Screen.SetResolution((int)(currentScreenRes.x * resolutionFactor), (int)(currentScreenRes.y * resolutionFactor), fullScreen);

                currentDynamicScreenRes = new Vector2(currentScreenRes.x * resolutionFactor, currentScreenRes.y * resolutionFactor);
            }
        }

        //INFERIOR
        else if (meanFramerate > targetFrameRate + frameRateGap)
        {
            if (resolutionFactor < 1f)
            {
                if (dynamicResolutionDebug)
                    Debug.Log(meanFramerate + " is > " + (targetFrameRate + frameRateGap).ToString());
                
                resolutionFactor += frameRateOffset;

                Screen.SetResolution((int)(currentScreenRes.x * resolutionFactor), (int)(currentScreenRes.y * resolutionFactor), fullScreen);

                currentDynamicScreenRes = new Vector2(currentScreenRes.x * resolutionFactor, currentScreenRes.y * resolutionFactor);
            }
        }

        //SAME
        else
        {
            if (dynamicResolutionDebug)
                Debug.Log(meanFramerate + " is ~ " + (targetFrameRate).ToString());
        }

        if (dynamicResolutionDebug)
            Debug.Log(meanFramerate + " - " + resolutionFactor);

        yield return new WaitForSecondsRealtime(frameRatePause);

        isAnalysingFrameRate = false;

        StartCoroutine(AnalyseFramerate());
    }

    public void ResetDynamicResolution()
    {
        
    }
}