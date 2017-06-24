// Used by the tree-chopping, mining demo scene
// to track how much wood has been gathered,
// and print it to the screen

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WoodCounter : MonoBehaviour {
	public Text woodCounter;
	public int woodToAddPerChop = 5;
	private int totalWoodGathered = 0;


	public void AddWood(){
		totalWoodGathered = totalWoodGathered + woodToAddPerChop;
		woodCounter.text = "TOTAL WOOD GATHERED: " + totalWoodGathered;
	}

}
