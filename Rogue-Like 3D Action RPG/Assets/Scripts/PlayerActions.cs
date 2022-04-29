using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour {
	
	//public Weapon weapon;
	
	CapsuleCollider ccol;
	
	Rigidbody rb;
	Animator anim;
	
	//Stats
	public float g_maxSpeed, a_maxSpeed, g_acceleration, a_acceleration, jumpHeight, rotationSpeed;
	
	float maxUp, jump, dodge, light, heavy, special;
	
	Transform cam;
	
	Vector3 preVel;
	
	
	void Start () {
		cam = Camera.main.transform;
		ccol = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
	}
	
	void LateUpdate () {
		rb.velocity = new Vector3 (rb.velocity.x, preVel.y, rb.velocity.z);
		
		//Check for Ground
		RaycastHit groundHit;
		Physics.SphereCast(transform.position, 0.2f, Vector3.down, out groundHit, 1f);
		
		//Actions **This will be replaced with triggers from the *NEW* input system
		if (Input.GetButtonDown("Jump")) {
			jump = 0.375f;
		}
		if (Input.GetButtonDown("Dodge")) {
			dodge = 0.375f;
		}
		if (Input.GetButtonDown("LightAttack")) {
			light = 0.375f;
		}
		if (Input.GetButtonDown("HeavyAttack")) {
			heavy = 0.375f;
		}
		if (Input.GetButtonDown("Special")) {
			special = 0.375f;
		}
		
		anim.SetBool("Dodge", dodge>0);
		anim.SetBool("Jump", jump>0);
		anim.SetBool("LightAttack", light>0);
		anim.SetBool("HeavyAttack", heavy>0);
		anim.SetBool("Special", special>0);
		
		if (!anim.IsInTransition(0)) {
			dodge =	Mathf.Clamp01(dodge-Time.deltaTime);
			light =	Mathf.Clamp01(light-Time.deltaTime);
			heavy = Mathf.Clamp01(heavy-Time.deltaTime);
			special = Mathf.Clamp01(special-Time.deltaTime);
			jump = Mathf.Clamp01(jump-Time.deltaTime);
		}
		
		//Rotation
		if (new Vector2 (Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude > 0) {
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(((cam.right*Input.GetAxis("Horizontal")) + (new Vector3(cam.forward.x, 0, cam.forward.z).normalized*Input.GetAxis("Vertical"))).normalized), rotationSpeed*0.1f*Time.deltaTime);
		}
		
		//Test if Action
		if ((!anim.GetCurrentAnimatorStateInfo(0).IsName("Ground") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Air"))) {
			Action(groundHit);
			//Save Velocity
			if (!anim.IsInTransition(0)) {
				preVel = rb.velocity;
			}
			return;
		}
		
		//Set Velocity
		rb.velocity = preVel;
		
		//Call normal Movement code
		if (groundHit.collider != null && rb.velocity.y <= 0 && groundHit.normal.y > 0.707f) {
			Ground (groundHit);
		} else {
			Air ();
		}
		
		//Save Velocity
		if (!anim.IsInTransition(0)) {
			preVel = rb.velocity;
		}
		
		anim.SetFloat("Speed", new Vector2 (rb.velocity.x, rb.velocity.z).magnitude/g_maxSpeed);
	}
	
	void Ground (RaycastHit groundPoint) {
		//Set Animator State
		if (!anim.GetBool("OnGround")) {
			anim.SetBool("OnGround", true);
		}
		
		//Set rb Constraints
		rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
		//Stick To Ground
		transform.position = new Vector3(transform.position.x, groundPoint.point.y, transform.position.z) + (Vector3.up*0.75f);
		
		//Change Speed With Surface Normal
		RaycastHit surNor;
		Physics.Raycast(new Vector3(groundPoint.point.x, transform.position.y, groundPoint.point.z), Vector3.down, out surNor, 2);
		//Move
		rb.velocity = Vector3.Lerp(rb.velocity, Vector3.ClampMagnitude(((cam.right*Input.GetAxis("Horizontal")) + (new Vector3(cam.forward.x, 0, cam.forward.z).normalized*Input.GetAxis("Vertical")))*g_maxSpeed, g_maxSpeed), g_acceleration*Time.deltaTime);
		
		/*Jump
		if (jump > 0) {
			jump = 0;
			//Set rb Constraints
			rb.constraints = RigidbodyConstraints.FreezeRotation;
			//Set Velocity
			rb.velocity = new Vector3 (rb.velocity.x, jumpHeight, rb.velocity.z);
		}*/
		maxUp = rb.velocity.y;
	}
	
	void Air () {
		//Set Animator State
		if (anim.GetBool("OnGround")) {
			anim.SetBool("OnGround", false);
		}
		
		//Set rb Constraints
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		
		//Move
		Vector3 vel = Vector3.ClampMagnitude(((cam.right*Input.GetAxis("Horizontal")) + (new Vector3(cam.forward.x, 0, cam.forward.z).normalized*Input.GetAxis("Vertical")))*a_maxSpeed, a_maxSpeed);
		rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(vel.x, rb.velocity.y, vel.z), a_acceleration*Time.deltaTime);
		
		rb.velocity = new Vector3 (rb.velocity.x, Mathf.Clamp(maxUp, -Mathf.Infinity, rb.velocity.y), rb.velocity.z);
		maxUp -= Physics.gravity.magnitude*Time.deltaTime;
	}
	
	void Action (RaycastHit groundPoint) {
		//Set rb Constraints
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		
		if (groundPoint.collider != null && groundPoint.normal.y > 0.707f && rb.velocity.y <= 0) {
			if (!anim.GetBool("OnGround")) {
				anim.SetBool("OnGround", true);
			}
				
			//Stick To Ground
			transform.position = new Vector3(transform.position.x, groundPoint.point.y, transform.position.z) + (Vector3.up*0.75f);
			maxUp = rb.velocity.y;
		} else {
			anim.SetBool("OnGround", false);
			rb.velocity = new Vector3 (rb.velocity.x, Mathf.Clamp(maxUp, -Mathf.Infinity, rb.velocity.y), rb.velocity.z);
			maxUp = Mathf.Clamp (maxUp-(Physics.gravity.magnitude*Time.deltaTime), -Mathf.Infinity, rb.velocity.y);
		}
	}
	
	void ResetAnimVar (int index) {
		switch (index) {
			case 0:
				jump = 0;
				break;
			case 1:
				dodge = 0;
				break;
			case 2:
				light = 0;
				break;
			case 3:
				heavy = 0;
				break;
			case 4:
				special = 0;
				break;
			default:
				break;
		};
	}
	
	void UpdateMaxRotationSpeed (float speed) {
		rotationSpeed = speed;
	}
	
	void AddUpwardForce () {
		rb.velocity = new Vector3 (rb.velocity.x, jumpHeight, rb.velocity.z);
		maxUp = jumpHeight;
		preVel = rb.velocity;
	}
}