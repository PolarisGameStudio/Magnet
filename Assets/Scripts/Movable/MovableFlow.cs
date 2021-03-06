﻿using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio;

public class MovableFlow : MovableScript 
{

	public override void Start ()
	{
		gameObject.tag = "Suggestible";
		slowMoTrigger = transform.GetComponentInChildren<SlowMotionTriggerScript> ();

		ToDeadlyColor ();
	}

	protected override void HitPlayer (Collision other)
	{
		if(other.collider.tag == "Player")
		{
			PlayersGameplay playerScript = other.collider.GetComponent<PlayersGameplay> ();

			if (playerScript.playerState == PlayerState.Dead)
				return;
			
			playerScript.Death (DeathFX.All, other.contacts [0].point);
			InstantiateParticles (other.contacts [0], GlobalVariables.Instance.HitParticles, GlobalVariables.Instance.playersColors [(int)playerScript.playerName]);

			PlayerKilled ();

			GlobalMethods.Instance.Explosion (transform.position);
		}
	}
}
