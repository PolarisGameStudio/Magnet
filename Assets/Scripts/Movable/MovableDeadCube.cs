﻿using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio;
using DG.Tweening;

public class MovableDeadCube : MovableScript
{
	public override void OnEnable ()
	{
		tag = "DeadCube";

		hold = false;

		rigidbodyMovable = GetComponent<Rigidbody>();
		movableRenderer = GetComponent<Renderer> ();
		cubeMeshFilter = transform.GetChild (2).GetComponent<MeshFilter> ();
		cubeMaterial = transform.GetChild (1).GetComponent<Renderer> ().material;
		slowMoTrigger = transform.GetComponentInChildren<SlowMotionTriggerScript> ();

		//cubeMaterial.SetFloat ("_Lerp", 1);
		//cubeMaterial.SetColor ("_Color", Color.black);

		cubeMeshFilter.mesh = GlobalVariables.Instance.cubesStripes [Random.Range (0, GlobalVariables.Instance.cubesStripes.Length)];
		attracedBy.Clear ();
		repulsedBy.Clear ();

		ToDeadlyColor (0.01f);
	}

	public override void Start ()
	{
		ToDeadlyColor (0.01f);
	}

	protected override void HitPlayer (Collision other)
	{
		if(other.collider.tag == "Player" && other.collider.GetComponent<PlayersGameplay>().playerState != PlayerState.Dead)
		{
			other.collider.GetComponent<PlayersGameplay> ().Death (DeathFX.All, other.contacts [0].point);

			InstantiateParticles (other.contacts [0], GlobalVariables.Instance.HitParticles, other.gameObject.GetComponent<Renderer>().material.color);

			GlobalMethods.Instance.Explosion (transform.position);
		}
	}
}
