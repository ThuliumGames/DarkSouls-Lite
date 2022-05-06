using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
	
	public Transform objToFollow, target, cam;
	
	public float lerp, offset, rotSpeed, maxDistance;
	
	public LayerMask lm;
	
	float ang;
	
	Vector3 prevLoc, vector;
	
	void FixedUpdate () {
		if (objToFollow==null) return;
		
		Vector3 point = objToFollow.position+Vector3.up;
		
		if (target!=null) {
			//point = (target.position+objToFollow.position)/2.0f;
			transform.LookAt(target.position+Vector3.up);
		}
		
		
		//Delay movement to smooth out jagged movement
		vector = Vector3.Lerp(vector, (prevLoc-point)*offset/60.0f, lerp/60.0f);
		
		//Undo Rotation
		transform.RotateAround(point+vector, transform.right, -ang);
		
		//Move to follow location
		transform.position = point+vector;
		
		//Set up down angle
		ang = Mathf.Clamp(ang-Input.GetAxis("Mouse Y")*rotSpeed/60.0f, -89, 89);
		transform.eulerAngles = new Vector3(ang, transform.eulerAngles.y, 0);
		
		//Set Left Right angle
		transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X")*rotSpeed/60.0f);
		
		//Test for wall
		RaycastHit h;
		Physics.Raycast(transform.position, -transform.forward, out h, maxDistance, lm);
		
		if (h.collider != null) {
			//Move out of wall
			transform.Translate(0, 0, -h.distance);
			
			//Make sure camera isnt in wal
			RaycastHit h2;
			Physics.Raycast(transform.position, h.normal, out h2, 0.3f, lm);
			
			cam.position = transform.position;
			
			if (h2.collider != null) {
				cam.Translate(h.normal*(h2.distance*0.5f));
			} else {
				cam.Translate(h.normal*0.3f);
			}
		} else {
			//reset position if not in wall
			transform.Translate(0, 0, -maxDistance);
			cam.position = transform.position;
		}
		
		cam.eulerAngles = transform.eulerAngles;
		
		prevLoc = point;
	}
}
