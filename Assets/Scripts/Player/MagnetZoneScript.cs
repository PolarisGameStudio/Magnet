﻿using UnityEngine;
using System.Collections;
using XboxCtrlrInput;
using Rewired;

public class MagnetZoneScript : MonoBehaviour 
{
	public float rayLength;

	private Transform character;
	private PlayersGameplay characterScript;

	private RaycastHit objectHit;

	public Player player;

	// Use this for initialization
	void Start () 
	{
		character = gameObject.transform.parent;
		characterScript = character.GetComponent<PlayersGameplay> ();
	}

	void Update ()
	{
		player = character.GetComponent<PlayersGameplay> ().player;
	}

	void OnTriggerStay (Collider other)
	{
		if(GlobalVariables.Instance.GameOver == false && GlobalVariables.Instance.GamePaused == false)
		{
			if(other.tag == "Movable" && character.GetComponent<PlayersGameplay>().playerState != PlayerState.Holding && character.GetComponent<PlayersGameplay>().playerState != PlayerState.Stunned && character.GetComponent<PlayersGameplay>().playerState != PlayerState.Dead
				|| other.tag == "Fluff" && character.GetComponent<PlayersGameplay>().playerState != PlayerState.Holding && character.GetComponent<PlayersGameplay>().playerState != PlayerState.Stunned && character.GetComponent<PlayersGameplay>().playerState != PlayerState.Dead)
			{
				if(Physics.Raycast(character.transform.position, other.transform.position - character.transform.position, out objectHit, rayLength))
				{
					Debug.DrawRay(character.transform.position, other.transform.position - character.transform.position, Color.red);


					if(player != null)
					{
						if(objectHit.transform.tag == "Movable" && player.GetButton("Attract"))
						{
							characterScript.Attraction (objectHit.collider.gameObject);

							if (!other.GetComponent<MovableScript> ().attracedBy.Contains (character.gameObject))
								other.GetComponent<MovableScript> ().attracedBy.Add (character.gameObject);
						}

						if(objectHit.transform.tag == "Movable" && player.GetButton("Repulse"))
						{
							characterScript.Repulsion (objectHit.collider.gameObject);	

							if (!other.GetComponent<MovableScript> ().repulsedBy.Contains (character.gameObject))
								other.GetComponent<MovableScript> ().repulsedBy.Add (character.gameObject);
						}

						if(objectHit.transform.tag == "Fluff" && player.GetButton("Repulse"))
						{
							characterScript.Repulsion (objectHit.collider.gameObject);
						}
					}
				}
			}
		}
	}

	void OnTriggerExit (Collider other)
	{
		if(GlobalVariables.Instance.GameOver == false && GlobalVariables.Instance.GamePaused == false)
		{
			if (other.tag =="Movable" && other.GetComponent<MovableScript> ().attracedBy.Contains (character.gameObject))
				other.GetComponent<MovableScript> ().attracedBy.Remove (character.gameObject);

			if (other.tag =="ThrownMovable" && other.GetComponent<MovableScript> ().attracedBy.Contains (character.gameObject))
				other.GetComponent<MovableScript> ().attracedBy.Remove (character.gameObject);


			if (other.tag =="Movable" && other.GetComponent<MovableScript> ().repulsedBy.Contains (character.gameObject))
				other.GetComponent<MovableScript> ().repulsedBy.Remove (character.gameObject);

			if (other.tag =="ThrownMovable" && other.GetComponent<MovableScript> ().repulsedBy.Contains (character.gameObject))
				other.GetComponent<MovableScript> ().repulsedBy.Remove (character.gameObject);
		}
	}
}
