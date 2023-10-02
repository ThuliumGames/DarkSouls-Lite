using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCancel : MonoBehaviour {
	
	Movement m;
	
	void Start () {
		m = GetComponentInParent<Movement>();
	}
	
	void cancelMove () {
			m.canMove = true;
	}
	
	void restartMove () {
		m.a1 = 0;
		m.a2 = 0;
		m.canMove = false;
	}
	
	void canTransition () {
		m.anim.SetBool("Trans", true);
	}
	
	void Jump (float jumpForce) {
		m.DoJump(Mathf.Floor(jumpForce)/10f, ((jumpForce)-Mathf.Floor(jumpForce))>0);
	}
	
	void ChangeGravity (float gravityScale) {
		m.ChangeGravity(gravityScale);
	}
	
	void ActionControl (float speed) {
		m.ActionControl(speed);
	}
	
	void MakeGround () {
		m.MakeGround();
	}
	
	void MakeAir () {
		m.MakeAir();
	}
	
	void MakeRoll () {
		m.MakeRoll();
	}
	
	void MakeDive () {
		m.MakeDive();
	}
	
	void Charge (int level) {
		m.chargeLevel = Mathf.Clamp(m.chargeLevel+level, 0, (float)m.charge_Damages.Length-2);
	}
	
	void MakeHeavy (int mass) {
		m.rb.mass = mass;
	}
}
