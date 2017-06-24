using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningObject : MonoBehaviour {

	[Header("SET TOTAL AMOUNT")]
	public int totalAmount = 50;
	[Header("SET AMOUNT TO SUBTRACT EACH HIT")]
	public int deductAmount = 5;
	[Header("DESTROY WHEN DEPLETED?")]
	public bool destroyIfEmpty = true;
	[Header("PLAY EFFECT WHEN DEPLETED? (Requires Effects Builder)")]
	public bool playEffectWhenDepleted = false;
	public string effectsBuilderName = "";
	private AIDesigner.EffectsBuilder ebx;

	// Use this for initialization
	void Start () {
		ebx = GetComponent<AIDesigner.EffectsBuilder>();
	}

	public void Subtract(){
		
		totalAmount = Mathf.Clamp((totalAmount - deductAmount), 0, totalAmount);

		if (totalAmount <= 0){
			if (destroyIfEmpty){

				Destroy(gameObject);
			}
 
			if (playEffectWhenDepleted && ebx != null && effectsBuilderName != ""){

				ebx.Play(effectsBuilderName);
			}
		}
	}
}