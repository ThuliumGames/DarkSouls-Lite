using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanJump : StateMachineBehaviour {
	
	override public void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!animator.IsInTransition(0)) {
			animator.gameObject.SendMessage ("cancelMove");
		}		
	}
}
