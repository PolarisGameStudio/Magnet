﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArenaCorner : MonoBehaviour
{
    public List<Transform> deadlyColumns = new List<Transform>();

    public List<GameObject> touchingGameobjects = new List<GameObject>();

    private ArenaDeadzones arenaDeadzones;

    private float campingDuration = 5f;

    void Start()
    {
        arenaDeadzones = FindObjectOfType<ArenaDeadzones>();

        GlobalVariables.Instance.OnEndMode += () => StopAllCoroutines();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (GlobalVariables.Instance.CurrentModeLoaded == WhichMode.Tutorial)
            return;

        if (collider.GetComponent<Rigidbody>() == null)
            return;
		
        if (collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (!touchingGameobjects.Contains(collider.gameObject))
            {
                touchingGameobjects.Add(collider.gameObject);

                StartCoroutine(WaitForCamping(collider.gameObject));
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (GlobalVariables.Instance.CurrentModeLoaded == WhichMode.Tutorial)
            return;
		
        if (collider.GetComponent<Rigidbody>() == null)
            return;
		
        if (touchingGameobjects.Contains(collider.gameObject))
            touchingGameobjects.Remove(collider.gameObject);
    }

    IEnumerator WaitForCamping(GameObject player)
    {
        yield return new WaitForSeconds(campingDuration);

        yield return new WaitWhile(() => GlobalVariables.Instance.GameState != GameStateEnum.Playing);

        if (player == null)
        {
            touchingGameobjects.Remove(player); 
            yield break;
        }

        if (touchingGameobjects.Contains(player))
        {
            var s = player.GetComponent<PlayersGameplay>();

            if (s.playerState == PlayerState.Dead || s.playerState == PlayerState.Stunned)
                yield break;

            foreach (var c in deadlyColumns)
                StartCoroutine(arenaDeadzones.SetDeadly(c));
        }

    }
}
