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
	
	float maxUp, jump, dodge, a1, a2, a3, special, maxSlope;
	
	int currentMove = 1;
	
	Transform cam;
	
	Vector3 preVel;
	Vector2 savedVel;
	
	public RuntimeAnimatorController[] av;
	
	void Start () {
		cam = Camera.main.transform;
		ccol = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
	}
	
	void LateUpdate () {
		
		rb.velocity = new Vector3 (Mathf.Lerp(rb.velocity.x, savedVel.x, anim.GetFloat("Keep Velocity")), preVel.y, Mathf.Lerp(rb.velocity.z, savedVel.y, anim.GetFloat("Keep Velocity")));
		
		//Reset Move Canceling
		if (anim.IsInTransition(0)) {
			anim.SetFloat("Roll Cancel", 0);
			anim.SetFloat("Jump Cancel", 0);
			anim.SetFloat("Attack Cancel", 0);
		}
		
		//Check for Ground
		RaycastHit groundHit;
		Physics.SphereCast(transform.position+Vector3.up, 0.2f, Vector3.down, out groundHit, 1.25f);
		
		//Actions **This will be replaced with triggers from the *NEW* input system
		if (Input.GetButtonDown("Jump")) {
			jump = 0.375f;
		}
		if (Input.GetButtonDown("Dodge")) {
			dodge = 0.375f;
		}
		
		if (Input.GetButtonDown("Attack" + currentMove)) {
			a1 = 0.375f;
		}
		
		if (currentMove == 1) {
			if (Input.GetButtonDown("Attack2")) {
				a2 = 0.375f;
			}
			if (Input.GetButtonDown("Attack3")) {
				a3 = 0.375f;
			}
		}
		if (Input.GetButtonDown("Special")) {
			special = 0.375f;
		}
		
		if (!anim.IsInTransition(0)) {
			anim.SetBool("Dodge", dodge>0);
			anim.SetBool("Jump", jump>0);
			anim.SetBool("Attack1", a1>0);
			anim.SetBool("Attack2", a2>0);
			anim.SetBool("Attack3", a3>0);
			anim.SetBool("Special", special>0);
		}
		
		if (!anim.IsInTransition(0)) {
			dodge =	Mathf.Clamp01(dodge-Time.deltaTime);
			a1 =	Mathf.Clamp01(a1-Time.deltaTime);
			a2 = Mathf.Clamp01(a2-Time.deltaTime);
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
			
			if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude > savedVel.magnitude) {
				savedVel = new Vector2(rb.velocity.x, rb.velocity.z);
			} else {
				savedVel /= (Time.deltaTime*2)+1f;
				if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude > 0) {
					savedVel = new Vector2(rb.velocity.x, rb.velocity.z).normalized*savedVel.magnitude;
				}
			}
			return;
		}
		
		//Set Velocity
		rb.velocity = preVel;
		
		//Call normal Movement code
		if (groundHit.collider != null && rb.velocity.y <= 0 && groundHit.normal.y > maxSlope) {
			if (groundHit.normal.y <= 0.707f) {
				maxSlope += Time.deltaTime*(1-groundHit.normal.y)*3;
			} else {
				maxSlope = 0;
			}
			
			Ground (groundHit);
		} else {
			
			maxSlope = 0.707f;
			
			Air ();
		}
		
		//Save Velocity
		if (!anim.IsInTransition(0)) {
			preVel = rb.velocity;
		}
		
		if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude > savedVel.magnitude) {
			savedVel = new Vector2(rb.velocity.x, rb.velocity.z);
		} else {
			savedVel /= (Time.deltaTime*2)+1f;
			if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude > 0) {
				savedVel = new Vector2(rb.velocity.x, rb.velocity.z).normalized*savedVel.magnitude;
			}
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
		
		//Change Speed With Surface Normal
		float slope = (1-groundPoint.normal.y);
		
		//do new raycast to prevent inaccurate normals at corners
		RaycastHit trueNormal;
		Physics.Raycast (groundPoint.point+Vector3.up, Vector3.down, out trueNormal, 2f);
		
		float speedMulti = Mathf.Clamp01(1-(slope*(Mathf.Clamp01(-Vector3.Dot(transform.forward, new Vector3 (trueNormal.normal.x, 0, trueNormal.normal.z).normalized))))*1.5f);
		
		//Stick To Ground
		transform.position = new Vector3(transform.position.x, groundPoint.point.y, transform.position.z)-(Vector3.up*Mathf.Clamp(slope, 0, 0.25f));
		
		//Move
		rb.velocity = Vector3.Lerp(rb.velocity, Vector3.ClampMagnitude(((cam.right*Input.GetAxis("Horizontal")) + (new Vector3(cam.forward.x, 0, cam.forward.z).normalized*Input.GetAxis("Vertical")))*g_maxSpeed*speedMulti, g_maxSpeed), g_acceleration*Time.deltaTime);
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
		
		if (groundPoint.collider != null && groundPoint.normal.y > maxSlope && rb.velocity.y <= 0) {
			
			if (groundPoint.normal.y <= 0.707f) {
				maxSlope += Time.deltaTime*(1-groundPoint.normal.y)*3;
			} else {
				maxSlope = 0;
			}
			
			if (!anim.GetBool("OnGround")) {
				anim.SetBool("OnGround", true);
			}
				
			//Change Speed With Surface Normal
			float slope = (1-groundPoint.normal.y);
			
			//do new raycast to prevent inaccurate normals at corners
			RaycastHit trueNormal;
			Physics.Raycast (groundPoint.point+Vector3.up, Vector3.down, out trueNormal, 2f);
			
			float speedMulti = Mathf.Clamp01(1-(slope*(Mathf.Clamp01(-Vector3.Dot(transform.forward, new Vector3 (trueNormal.normal.x, 0, trueNormal.normal.z).normalized))))*1.5f);
			
			//Stick To Ground
			transform.position = new Vector3(transform.position.x, groundPoint.point.y, transform.position.z)-(Vector3.up*Mathf.Clamp(slope, 0, 0.25f));
			maxUp = rb.velocity.y;
			
			rb.velocity = new Vector3 (rb.velocity.x*speedMulti*(g_maxSpeed/7.0f), rb.velocity.y, rb.velocity.z*speedMulti*(g_maxSpeed/7.0f));
			
		} else {
			
			maxSlope = 0.707f;
			
			anim.SetBool("OnGround", false);
			rb.velocity = new Vector3 (rb.velocity.x, Mathf.Clamp(maxUp, -Mathf.Infinity, rb.velocity.y), rb.velocity.z);
			maxUp = Mathf.Clamp (maxUp-(Physics.gravity.magnitude*Time.deltaTime), -Mathf.Infinity, rb.velocity.y);
			
			rb.velocity = new Vector3 (rb.velocity.x*(a_maxSpeed/9.0f), rb.velocity.y, rb.velocity.z*(a_maxSpeed/9.0f));
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
				a1 = 0;
				break;
			case 3:
				a2 = 0;
				break;
			case 4:
				a3 = 0;
				break;
			case 5:
				special = 0;
				break;
			default:
				break;
		};
	}
	
	void UpdateMaxRotationSpeed (float speed) {
		rotationSpeed = speed;
	}
	
	void AddUpwardForce (float height) {
		rb.velocity = new Vector3 (rb.velocity.x, jumpHeight*height, rb.velocity.z);
		maxUp = jumpHeight*height;
		preVel = rb.velocity;
	}
	
	void Return () {
		currentMove = 1;
		anim.runtimeAnimatorController = av[0];
	}
	
	void Attack1 (string StartingAnim) {
		currentMove = 1;
		anim.runtimeAnimatorController = av[1];
		anim.Play(StartingAnim);
	}
	
	void Attack2 (string StartingAnim) {
		currentMove = 2;
		anim.runtimeAnimatorController = av[2];
		anim.Play(StartingAnim);
	}
	
	void Attack3 (string StartingAnim) {
		currentMove = 3;
		anim.runtimeAnimatorController = av[3];
		anim.Play(StartingAnim);
	}
}