﻿using UnityEngine;
using System.Collections;

public class SpawnSingletonManager : MonoBehaviour 
{
	void Awake ()
	{
		Debug.Log(StaticVariables.Instance.gameObject);
		Debug.Log(GamepadsManager.Instance.gameObject);
		Debug.Log(LoadModeManager.Instance.gameObject);
	}

}
