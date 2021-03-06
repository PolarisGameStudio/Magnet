﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement_TowardsCube : AIMovement_Towards 
{
	private int randomCubes = 4;

	protected override void OnEnable ()
	{
		AIScript.shootTarget = null;

		if (!CanPlay ())
			return;
		
		base.OnEnable ();
	}


	protected override void Enable ()
	{
		if (!AIScript.movementLayerEnabled)
			return;

		if (!CanPlay ())
			return;
		
		base.Enable ();

		if (AIScript.closerCubes.Count == 0)
			return;

		if(AIScript.closerPlayers.Count >= randomCubes)
			AIScript.holdTarget = target = AIScript.closerCubes [Random.Range (0, randomCubes)].transform;
		else
			AIScript.holdTarget = target = AIScript.closerCubes [0].transform;
	}

	protected override void Update ()
	{
		if (!AIScript.movementLayerEnabled)
			return;

		if (!CanPlay ())
			return;

		base.Update ();

		if (AIScript.closerCubes.Count == 0)
			return;
		
		if(target == null || target.tag != "Movable")
		{
			if(AIScript.closerPlayers.Count >= randomCubes)
				AIScript.holdTarget = target = AIScript.closerCubes [Random.Range (0, randomCubes)].transform;
			else
				AIScript.holdTarget = target = AIScript.closerCubes [0].transform;
		}
	}

	protected override void OnDisable ()
	{
		//AIScript.cubeTarget = null;
		target = null;
		
		base.OnDisable ();
	}
}
