﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class ZoomCamera : MonoBehaviour 
{
	public List<ZoomSettings> zoomList = new List<ZoomSettings> ();

	[Header ("Ease")]
	public Ease zoomEase = Ease.OutQuad;

	[Header ("Test")]
	public FeedbackType whichZoomTest;
	public bool test = false;

	private Camera cam;
	private float initialFOV;

	// Use this for initialization
	void Awake () 
	{
		cam = GetComponent<Camera> ();
		initialFOV = cam.fieldOfView;

		GlobalVariables.Instance.OnStartupDone += () => Zoom (FeedbackType.Startup);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(test)
		{
			test = false;
			Zoom (whichZoomTest);
		}
	}

	public void Zoom (FeedbackType whichZoom = FeedbackType.Default)
	{
		StartCoroutine (ZoomCoroutine (whichZoom));
	}

	IEnumerator ZoomCoroutine (FeedbackType whichZoom)
	{
		if (DOTween.IsTweening ("ZoomCamera"))
			DOTween.Kill ("ZoomCamera");

		float newFOV = 0;
		float zoomDuration = 0;
		float resetDuration = 0;

		for(int i = 0; i < zoomList.Count; i++)
		{
			if(zoomList [i].whichZoom == whichZoom)
			{
				newFOV = zoomList [i].newFOV;
				zoomDuration = zoomList [i].zoomDuration;
				resetDuration = zoomList [i].resetDuration;
				break;
			}
		}

		Tween tween = cam.DOFieldOfView (newFOV, zoomDuration).SetEase (zoomEase).SetId ("ZoomCamera");

		yield return tween.WaitForCompletion ();

		tween = cam.DOFieldOfView (initialFOV, resetDuration).SetEase (zoomEase).SetId ("ZoomCamera");
	}

}

[Serializable]
public class ZoomSettings
{
	public FeedbackType whichZoom = FeedbackType.Default;

	public float newFOV;
	public float zoomDuration;
	public float resetDuration;
}
