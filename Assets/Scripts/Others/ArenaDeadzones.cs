﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Replay;
using Sirenix.OdinInspector;

public class ArenaDeadzones : MonoBehaviour
{
    public delegate void ColumnDeadly(Transform column);

    public enum RandomType
    {
        Single,
        AllSettings}

    ;

    [Header("Random")]
    public List<WhichMode> allSettingsModes = new List<WhichMode>();

    private RandomType randomType = RandomType.Single;

    [Header("Settings")]
    public Ease ease = Ease.OutQuad;
    public List<ArenaDeadzonesSettings> deadzonesSettings = new List<ArenaDeadzonesSettings>();

    [Header("Single")]
    public float currentInterval;
    public float singleDelay = 0;
    public Vector2 intervalLimits = new Vector2(10, 1);
    public float reducedTime = 1f;

    [Header("Normal State")]
    public float normalEmission = 1f;

    [Header("Deadly State")]
    public Color deadlyColor;
    public float deadlyEmission = 2f;
    public float zScale = 3f;
    public float xScale = 1.8f;
    public float transitionDuration;

    [Header("Deadly Transition")]
    public List<DeadlyTransition> deadlyTransition = new List<DeadlyTransition>();

    [Header("Columns")]
    public Transform[] frontColumns = new Transform[27];
    public Transform[] backColumns = new Transform[27];
    public Transform[] rightColumns = new Transform[17];
    public Transform[] leftColumns = new Transform[17];

    [HideInInspector]
    public List<GameObject> deadlyColumns = new List<GameObject>();

    private List<Transform> allColumns = new List<Transform>();
    private string initialTag;
    private Vector3 initialScale;
    private Vector3 initialScale2;

    // Use this for initialization
    void Start()
    {
        initialTag = frontColumns[0].GetChild(0).tag;
        initialScale = frontColumns[0].GetChild(0).localScale;
        initialScale2 = frontColumns[0].GetChild(1).localScale;

        allColumns.AddRange(frontColumns);
        allColumns.AddRange(backColumns);
        allColumns.AddRange(rightColumns);
        allColumns.AddRange(leftColumns);

        GlobalVariables.Instance.OnEnvironementChromaChange += Reset;

        GlobalVariables.Instance.OnStartMode += Setup;
        GlobalVariables.Instance.OnRestartMode += Setup;
        GlobalVariables.Instance.OnMenu += Reset;

        Setup();
    }

    public void Setup()
    {
        StopAllCoroutines();

        Reset();

        int settingsIndex = 0;

        if (allSettingsModes.Contains(GlobalVariables.Instance.CurrentModeLoaded))
            randomType = RandomType.AllSettings;
        else
            randomType = RandomType.Single;

        switch (randomType)
        {
            case RandomType.Single:
                StartCoroutine(SingleDeadzone());
                break;
            case RandomType.AllSettings:
                settingsIndex = Random.Range(0, deadzonesSettings.Count);
                break;
        }

        if (randomType != RandomType.Single)
        {
            StartCoroutine(Deadzones(deadzonesSettings[settingsIndex].frontDelay, deadzonesSettings[settingsIndex].duration, deadzonesSettings[settingsIndex].frontIndex, frontColumns));
            StartCoroutine(Deadzones(deadzonesSettings[settingsIndex].backDelay, deadzonesSettings[settingsIndex].duration, deadzonesSettings[settingsIndex].backIndex, backColumns));
            StartCoroutine(Deadzones(deadzonesSettings[settingsIndex].rightDelay, deadzonesSettings[settingsIndex].duration, deadzonesSettings[settingsIndex].rightIndex, rightColumns));
            StartCoroutine(Deadzones(deadzonesSettings[settingsIndex].leftDelay, deadzonesSettings[settingsIndex].duration, deadzonesSettings[settingsIndex].leftIndex, leftColumns));
        }
    }

    [ButtonAttribute]
    public void Reset()
    {
        StopAllCoroutines();
        DOTween.Kill("ArenaDeadzone");
        deadlyColumns.Clear();

        foreach (var column in allColumns)
        {
            for (int i = 0; i < column.childCount; i++)
            {
                column.GetChild(i);

                column.GetChild(i).tag = initialTag;
                column.GetChild(i).GetComponent<Collider>().enabled = true;

                Color color = GlobalVariables.Instance.arenaColors[(int)GlobalVariables.Instance.environementChroma] * normalEmission;

                column.GetChild(i).GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
                column.GetChild(i).GetComponent<Renderer>().material.color = color;

                column.GetChild(i).localScale = initialScale;

                if (i == 0)
                    column.GetChild(i).localScale = initialScale;
                else
                    column.GetChild(i).localScale = initialScale2;
            }
        }
    }

    IEnumerator Deadzones(float delay, float duration, int[] indexes, Transform[] columns)
    {
        yield return new WaitForSeconds(delay);

        float waitTime = duration / columns.Length;

        for (int i = 0; i < columns.Length; i++)
        {
            yield return new WaitUntil(() => GlobalVariables.Instance.GameState == GameStateEnum.Playing);

            yield return new WaitWhile(() => ReplayManager.Instance.isReplaying);

            for (int j = 0; j < indexes.Length; j++)
            {
                if (indexes[j] == i)
                    StartCoroutine(SetDeadly(columns[j]));
            }

            yield return new WaitForSeconds(waitTime);
        }

        if (GlobalVariables.Instance.CurrentModeLoaded == WhichMode.Crush)
            SteamAchievements.Instance.UnlockAchievement(AchievementID.ACH_CRUSH);
    }

    IEnumerator SingleDeadzone()
    {
        currentInterval = intervalLimits.x;

        yield return new WaitForSeconds(singleDelay);

        int columnsCount = allColumns.Count;

        for (int i = 0; i < columnsCount; i++)
        {
            yield return new WaitUntil(() => GlobalVariables.Instance.GameState == GameStateEnum.Playing);

            yield return new WaitWhile(() => ReplayManager.Instance.isReplaying);

            Transform column = null;

            do
            {
                column = allColumns[Random.Range(0, allColumns.Count)];
            }
            while(column.GetChild(0).tag == "DeadZone");

            StartCoroutine(SetDeadly(column));

            yield return new WaitForSeconds(currentInterval);

            currentInterval -= reducedTime;

            if (currentInterval < intervalLimits.y)
                currentInterval = intervalLimits.y;
        }
    }

    public IEnumerator SetDeadly(Transform column, bool replaying = false)
    {
        if (!replaying)
            ReplayManager.Instance.arenaDeadzoneColumns.Add(new ReplayManager.ArenaDeadzoneColumn(column));

        foreach (var d in deadlyTransition)
        {
            if (d.toRed)
            {
                for (int i = 0; i < column.childCount; i++)
                {
                    column.GetChild(i).GetComponent<Renderer>().material.DOColor(deadlyColor * deadlyEmission, "_EmissionColor", d.duration).SetUpdate(false).SetId("ArenaDeadzone");
                    column.GetChild(i).GetComponent<Renderer>().material.DOColor(deadlyColor, d.duration).SetUpdate(false).SetId("ArenaDeadzone");

                    /*	if(i == 0)
						column.GetChild (i).DOScaleX (xScale, d.duration).SetUpdate (false);
					else
						column.GetChild (i).DOScaleX (1.3f, d.duration).SetUpdate (false);*/
                }
            }
            else
            {
                for (int i = 0; i < column.childCount; i++)
                {
                    Color color = GlobalVariables.Instance.arenaColors[(int)GlobalVariables.Instance.environementChroma];

                    column.GetChild(i).GetComponent<Renderer>().material.DOColor(color * deadlyEmission, "_EmissionColor", d.duration).SetUpdate(false).SetId("ArenaDeadzone");
                    column.GetChild(i).GetComponent<Renderer>().material.DOColor(color, d.duration).SetUpdate(false).SetId("ArenaDeadzone");
					
                    /*if(i == 0)
						column.GetChild (i).DOScale (initialScale, d.duration).SetUpdate (false);
					else
						column.GetChild (i).DOScale (initialScale2, d.duration).SetUpdate (false);*/
                }
            }

            float wait = d.waitDuration > 0 ? d.waitDuration : d.duration;
            yield return new WaitForSeconds(wait);
        }

        for (int i = 0; i < column.childCount; i++)
        {
            column.GetChild(i);

            column.GetChild(i).tag = "DeadZone";
            column.GetChild(i).GetComponent<Collider>().enabled = true;

            column.GetChild(i).GetComponent<Renderer>().material.DOColor(deadlyColor * deadlyEmission, "_EmissionColor", transitionDuration).SetUpdate(false).SetId("ArenaDeadzone");
            column.GetChild(i).GetComponent<Renderer>().material.DOColor(deadlyColor, transitionDuration).SetUpdate(false).SetId("ArenaDeadzone");

            column.GetChild(i).DOScaleZ(zScale, transitionDuration).SetUpdate(false).SetId("ArenaDeadzone");

            if (i == 0)
                column.GetChild(i).DOScaleX(xScale, transitionDuration).SetUpdate(false).SetId("ArenaDeadzone");
            else
                column.GetChild(i).DOScaleX(1.3f, transitionDuration).SetUpdate(false).SetId("ArenaDeadzone");

            deadlyColumns.Add(column.GetChild(i).gameObject);
        }
    }

    [System.Serializable]
    public class DeadlyTransition
    {
        public bool toRed = true;
        public float duration;
        public float waitDuration;
    }
}

[System.Serializable]
public class ArenaDeadzonesSettings
{
    [Header("Duration")]
    public float duration;

    [Header("Delays")]
    public float frontDelay;
    public float backDelay;
    public float rightDelay;
    public float leftDelay;

    [Header("Indexes")]
    public int[] frontIndex = new int[27];
    public int[] backIndex = new int[27];
    public int[] rightIndex = new int[17];
    public int[] leftIndex = new int[17];
}
