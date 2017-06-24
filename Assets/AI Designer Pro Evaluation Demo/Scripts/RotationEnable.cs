/*
	Unlocks rotation ability on bots.
	This helps to fix the older "Patrol Setup" demo scene
	where the bots failed to rotate towards waypoints. Those
	bots used an older version (< v0.90) and had their rotation features locked.

	This script unlocks it for those bots that do not rotate.

	All bots created through the menu should not have this issue because
	they use the new version of the Movement Controller.

	If your bot isn't turning towards patrol waypoint (such as in the Patrol demo scene)
	simply attach this script and it should work.

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationEnable : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (GetComponent<AIDesigner.MovementController>() != null){
			GetComponent<AIDesigner.MovementController>().canRot = true;
		}
	}
	
}
