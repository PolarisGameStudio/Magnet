﻿using UnityEngine;
using System.Collections;
using Replay;

public class PlayersTraining : PlayersGameplay 
{
	[Header ("Training Settings")]
	public float timeBetweenSpawn = 1f;

	public override void Death (DeathFX deathFX, Vector3 deathPosition, GameObject killingPlayer = null)
	{
		if (ReplayManager.Instance.isReplaying)
			return;
		
		if(playerState != PlayerState.Dead && GlobalVariables.Instance.GameState == GameStateEnum.Playing)
		{
			OnDeathVoid ();
			GlobalVariables.Instance.screenShakeCamera.CameraShaking(FeedbackType.Death);
			GlobalVariables.Instance.zoomCamera.Zoom(FeedbackType.Death);

			if(holdState == HoldState.Holding)
			{
				playerState = PlayerState.Dead;
				Transform holdMovableTemp = null;

				for(int i = 0; i < transform.childCount; i++)
				{
					if(transform.GetChild(i).tag == "Movable" || transform.GetChild(i).tag == "HoldMovable")
					{
						holdMovableTemp = transform.GetChild (i);

						holdMovableTemp.gameObject.GetComponent<MovableScript>().hold = false;

						holdMovableTemp.transform.SetParent(null);
						holdMovableTemp.transform.SetParent(movableParent);
						holdMovableTemp.GetComponent<MovableScript>().AddRigidbody();
					}
				}
			}

			playerState = PlayerState.Dead;


			for(int i = 0; i < GetComponent<PlayersFXAnimations>().attractionRepulsionFX.Count; i++)
			{
				Destroy (GetComponent<PlayersFXAnimations> ().attractionRepulsionFX [i]);
			}

			GlobalMethods.Instance.SpawnExistingPlayerRandomVoid (gameObject, timeBetweenSpawn);

			playerState = PlayerState.None;
		}
	}
}
