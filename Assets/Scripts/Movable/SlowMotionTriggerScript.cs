﻿using UnityEngine;
using System.Collections;

public class SlowMotionTriggerScript : MonoBehaviour 
{
	public bool triggerEnabled = false;

	[HideInInspector]
	public GameObject playerThatThrew;

	private SlowMotionCamera slowMotionCamera;
	private MovableScript movableScript;

	void Start ()
	{
		slowMotionCamera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<SlowMotionCamera>();
		movableScript = transform.parent.GetComponent<MovableScript> ();
	}

	void OnTriggerEnter (Collider other)
	{
		if(triggerEnabled && other.gameObject.layer == LayerMask.NameToLayer ("Player"))
		{
			if (GlobalVariables.Instance.GameState != GameStateEnum.Playing)
				return;

			playerThatThrew = movableScript.playerThatThrew;
			
			if(triggerEnabled && other.tag == "Player" && other.gameObject != playerThatThrew)
			{
				triggerEnabled = false;
				
				slowMotionCamera.StartSlowMotion ();
				slowMotionCamera.ContrastVignette(transform.position);
			}			
		}
	}
}
