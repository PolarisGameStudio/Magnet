﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AIDash_Towards : AIComponent
{
	[Header ("Chances")]
	[Range (0, 100)]
	public float [] towardsChances = new float[3] { 100, 100, 100 };

	[Header ("Random")]
	public AIRandomAngle[] randomAngles = new AIRandomAngle[3];

	[Header ("Delay")]
	public Vector2 randomDelay = new Vector2 (0.05f, 0.5f);

	protected override void Enable ()
	{
		if (!AIScript.dashLayerEnabled)
			return;

		if (!CanPlay ())
			return;

		base.Enable ();

		StartCoroutine (Delay ());
	}

	IEnumerator Delay ()
	{
		yield return new WaitForSecondsRealtime (Random.Range (randomDelay.x, randomDelay.y));

		if (Random.Range (0, 100) > towardsChances [(int)AIScript.aiLevel])
			yield break;

		if (AIScript.dashState != DashState.CanDash)
			yield break;

		if (AIScript.cubeTarget != null && AIScript.playerTarget != null)
			yield break;

		if (AIScript.cubeTarget == null && AIScript.playerTarget == null)
			yield break;

		AIScript.dashState = DashState.Dashing;

		if (AIScript.playerTarget != null)
			AIScript.movement = (AIScript.playerTarget.position - transform.position).normalized;
		else
			AIScript.movement = (AIScript.cubeTarget.position - transform.position).normalized;

		AIScript.movement = Quaternion.AngleAxis (Mathf.Sign (Random.Range (-1f, -1f)) * Random.Range (randomAngles [(int)AIScript.aiLevel].randomAngleMin, randomAngles [(int)AIScript.aiLevel].randomAngleMax), Vector3.up) * AIScript.movement;

		AIScript.movement.Normalize ();

		AIScript.StartCoroutine ("Dash");
	}
}
