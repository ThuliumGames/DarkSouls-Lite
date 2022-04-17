using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour {
	
	//public Weapon weapon;
	
	int grounded;
	
	CapsuleCollider ccol;
	
	Rigidbody rb;
	
	//Stats
	public float maxSpeed, g_acceleration, a_acceleration, jumpHeight;
	
	Transform cam;
	
	void Start () {
		cam = Camera.main.transform;
		ccol = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
	}
	
	void Update () {
		//Check for Ground
		RaycastHit groundHit;
		Physics.Raycast(transform.position, Vector3.down, out groundHit, 1.2f);
		grounded = System.Convert.ToInt32(groundHit.collider != null);
		
		switch (grounded) {
			case 0 :
				Air ();
				break;
			case 1 :
				Ground (groundHit.point);
				break;
			default :
				break;
		};
		
	}
	
	void Ground (Vector3 groundPoint) {
		if (rb.velocity.y <= 0) {
			//Set rb Constraints
			rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
			//Set Collider Size
			ccol.center = Vector3.up*0.333f;
			ccol.height = 1.333f;
			//Stick To Ground
			transform.position = groundPoint + (Vector3.up*0.75f);
			
			//Move
			rb.velocity = Vector3.Lerp(rb.velocity, Vector3.ClampMagnitude(((cam.right*Input.GetAxis("Horizontal")) + (new Vector3(cam.forward.x, 0, cam.forward.z)*Input.GetAxis("Vertical")))*maxSpeed, maxSpeed), g_acceleration*Time.deltaTime);
			
			//Jump
			if (Input.GetButtonDown("Jump")) {
				//Set rb Constraints
				rb.constraints = RigidbodyConstraints.FreezeRotation;
				//Set Velocity
				rb.velocity = new Vector3 (rb.velocity.x, jumpHeight, rb.velocity.z);
			}
		}
	}
	
	void Air () {
		//Set rb Constraints
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		//Set Collider Size
		ccol.center = Vector3.zero;
		ccol.height = 2;
		
		//Move
		Vector3 vel = Vector3.ClampMagnitude(((cam.right*Input.GetAxis("Horizontal")) + (new Vector3(cam.forward.x, 0, cam.forward.z)*Input.GetAxis("Vertical")))*maxSpeed, maxSpeed);
		rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(vel.x, rb.velocity.y, vel.z), a_acceleration*Time.deltaTime);
	}
}
