using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
	
	Rigidbody rb;
	
	Animator anim;
	
	public float attack_Power, speed, rotationSpeed, stun;
	
	float stunTime, maxSlope;
	
	List<Collider> cantHit = new List<Collider>();
	
	public LayerMask lm;
	
	void Start () {
		rb = GetComponent<Rigidbody>();
		anim = GetComponentInChildren<Animator>();
	}
	
	void FixedUpdate () {
		Look();
		if (stunTime<=0) {
			rb.mass = 10f;
		} else {
			rb.mass = 100000;
			stunTime-=0.02f;
		}
		
			//Check for Ground
			RaycastHit hit;
			bool onGround = false;
			if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f+(System.Convert.ToInt32(anim.GetBool("OnGround"))*0.5f), lm)) {
				if (hit.normal.y > maxSlope&&rb.velocity.y<=0) {
					onGround = true;
				}
				if (hit.normal.y < 0.5f) {
					maxSlope += (0.02f);
				} else {
					maxSlope = 0;
				}
			}
			
			if (onGround) {
				//Setup
				rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
				anim.SetBool("OnGround", true);
				
				//Snap to Ground
				transform.position = new Vector3(transform.position.x, (hit.point.y+1f), transform.position.z);
			} else {
				stunTime = Mathf.Clamp(stunTime, 0.1f, 0);
				rb.constraints = RigidbodyConstraints.FreezeRotation;
				anim.SetBool("OnGround", false);
			}
			
			if (stunTime<=0) {
				Vector3 wantedVel = Vector3.Lerp(new Vector3(rb.velocity.x, 0, rb.velocity.z), transform.forward*anim.GetFloat("MoveVelocity")*speed, 5*0.02f);
				rb.velocity = new Vector3 (wantedVel.x, rb.velocity.y, wantedVel.z);
			}
	}
	
	void Look() {
		// get the player vector
		Vector3 vector = (Globals.player.position-transform.position).normalized;
		
		// if there is no input, exit
		if (Mathf.Abs(vector.x) < 0.01f && Mathf.Abs(vector.z) < 0.01f) return;

		// calculate the direction to look at
		Vector3 direction = new Vector3(vector.x, 0, vector.z);
		
		// smoothly rotate the object to look in the direction
		Quaternion targetRotation = Quaternion.LookRotation(direction);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * (0.02f));
	}
	
	void Hit (Vector3 damage) {
		//anim.enabled = false;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		anim.SetBool("OnGround", false);
		rb.velocity = (new Vector3(damage.x, new Vector2(damage.x, damage.z).magnitude/2f, damage.z).normalized*Mathf.Log(new Vector3(damage.x, new Vector2(damage.x, damage.z).magnitude/2f, damage.z).magnitude*100f, 1.5f))*3;
		stunTime = stun*Mathf.Log(new Vector3(damage.x, new Vector2(damage.x, damage.z).magnitude/2f, damage.z).magnitude*100f, 1.5f)/20.0f;
	}
	
	void OnTriggerStay (Collider other) {
		if (other.GetComponent<Movement>()) {
			if (other.GetComponent<Movement>().Hit(((new Vector3 (other.transform.position.x, 0, other.transform.position.z)-new Vector3 (transform.position.x, 0, transform.position.z)).normalized*(attack_Power/100.0f))+new Vector3(0, attack_Power, 0))) {
				cantHit.Add(other);
			}
		}
	}
	
	void OnTriggerExit(Collider other) {
		if (other.GetComponent<Movement>()) {
			cantHit.Clear();
		}
	}
}
