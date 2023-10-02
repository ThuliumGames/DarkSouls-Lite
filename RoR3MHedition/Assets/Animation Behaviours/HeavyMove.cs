using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyMove : StateMachineBehaviour {
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.gameObject.SendMessage ("MakeHeavy", 10000);    
	}
	
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.gameObject.SendMessage ("MakeHeavy", 10);    
	}
}
