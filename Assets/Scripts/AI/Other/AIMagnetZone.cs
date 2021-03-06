﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Replay;

public class AIMagnetZone : MagnetZoneScript
{
    private AIGameplay AIScript;

    protected override void Start()
    {
        base.Start();

        AIScript = transform.GetComponentInParent <AIGameplay>();
    }

    protected override void Update()
    {
        if (!AIScript.isAttracting && playerScript.cubesAttracted.Count > 0)
            playerScript.cubesAttracted.Clear();
		
        if (!AIScript.isRepelling && playerScript.cubesRepulsed.Count > 0)
            playerScript.cubesRepulsed.Clear();
		
        if (!AIScript.isAttracting && !AIScript.isRepelling)
        {
            if (playerScript.cubesAttracted.Count > 0)
                playerScript.cubesAttracted.Clear();
           
            if (playerScript.cubesRepulsed.Count > 0)
                playerScript.cubesRepulsed.Clear();
        }			
    }

    protected override void OnTriggerStay(Collider other)
    {
        if (ReplayManager.Instance.isReplaying)
            return;
        
        if (playerScript.playerState == PlayerState.Startup || playerScript.playerState == PlayerState.Dead || playerScript.playerState == PlayerState.Stunned)
            return;

        if (GlobalVariables.Instance.GameState == GameStateEnum.Playing && playerScript.holdState == HoldState.CanHold)
        {
            if (other.tag == "Movable" || other.tag == "Suggestible")
            {
                RaycastHit hit;

                if (Physics.Raycast(player.transform.position, other.transform.position - player.transform.position, out hit, rayLength))
                {
                    if (hit.collider.gameObject.tag == "Movable" || hit.collider.gameObject.tag == "Suggestible")
                    {
                        //Debug.DrawRay(player.transform.position, other.transform.position - player.transform.position, Color.red);

                        if (AIScript.isAttracting && !AIScript.isRepelling)
                            Attract(other);

                        if (AIScript.isRepelling && !AIScript.isAttracting)
                            Repulse(other);						
                    }
                }
            }
        }
    }
}
