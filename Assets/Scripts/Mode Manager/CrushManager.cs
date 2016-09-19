﻿using UnityEngine;
using System.Collections;

public class CrushManager : MonoBehaviour 
{
	public GameObject[] playersList;

	public float timeBeforeEndGame = 2;

	private bool gameEndLoopRunning = false;

	void Start ()
	{
		GameObject[] allMovables = GameObject.FindGameObjectsWithTag ("Movable");

		for (int i = 0; i < allMovables.Length; i++)
		{
			allMovables [i].transform.GetChild(1).GetComponent<Renderer> ().enabled = false;	
			allMovables [i].transform.GetChild(2).GetComponent<Renderer> ().enabled = false;	
		}

		StartCoroutine (WaitForBeginning ());
	}

	IEnumerator WaitForBeginning ()
	{
		yield return new WaitWhile (() => GlobalVariables.Instance.GameState != GameStateEnum.Playing);

		GlobalMethods.Instance.StartCoroutine ("RandomPositionMovables", 0.1f);
	}

	// Update is called once per frame
	void Update () 
	{
		if(GlobalVariables.Instance.GameState == GameStateEnum.Playing)
			FindPlayers ();

	}

	void FindPlayers ()
	{
		playersList = GameObject.FindGameObjectsWithTag("Player");

		if(playersList.Length == 1 && gameEndLoopRunning == false)
		{
			gameEndLoopRunning = true;

			StartCoroutine (GameEnd ());
		}

		if(playersList.Length == 0 && gameEndLoopRunning == false)
		{
			gameEndLoopRunning = true;

			StartCoroutine (GameEndDraw ());
		}
	}

	IEnumerator GameEnd ()
	{
		switch (playersList [0].name)
		{
		case "Player 1":
			StatsManager.Instance.Winner(WhichPlayer.Player1);
			break;
		case "Player 2":
			StatsManager.Instance.Winner(WhichPlayer.Player2);
			break;
		case "Player 3":
			StatsManager.Instance.Winner(WhichPlayer.Player3);
			break;
		case "Player 4":
			StatsManager.Instance.Winner(WhichPlayer.Player4);
			break;
		}

		GlobalVariables.Instance.GameState = GameStateEnum.Over;

		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SlowMotionCamera>().StartEndGameSlowMotion();
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScreenShake>().CameraShaking();

		yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(timeBeforeEndGame));

		GameObject.FindGameObjectWithTag("MainMenuManager").GetComponent<MainMenuManagerScript>().GameOverMenuVoid ();
	}

	IEnumerator GameEndDraw ()
	{
		StatsManager.Instance.Winner(WhichPlayer.Draw);

		GlobalVariables.Instance.GameState = GameStateEnum.Over;

		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SlowMotionCamera>().StartEndGameSlowMotion();
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScreenShake>().CameraShaking();

		yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(timeBeforeEndGame));

		GameObject.FindGameObjectWithTag("MainMenuManager").GetComponent<MainMenuManagerScript>().GameOverMenuVoid ();
	}
	
}
