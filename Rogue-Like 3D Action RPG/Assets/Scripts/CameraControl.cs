using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
	
	public Transform objToFollow;
	
	public float maxDistance;
	
	public LayerMask lm;
	
	float ang;
	
	void LateUpdate () {
		if (objToFollow==null) return;
		
		transform.RotateAround(objToFollow.position, transform.right, -ang);
		
		transform.position = new Vector3 (objToFollow.position.x, Mathf.Lerp(transform.position.y, objToFollow.position.y, 10*Time.deltaTime), objToFollow.position.z);
		
		ang = Mathf.Clamp(ang-Input.GetAxis("Mouse Y"), -89, 89);
		
		transform.eulerAngles = new Vector3(ang, transform.eulerAngles.y, 0);
		transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X"));
		
		RaycastHit h;
		Physics.Raycast(transform.position, -transform.forward, out h, maxDistance, lm);
		
		if (h.collider != null) {
			transform.Translate(0, 0, -h.distance);
			//transform.Translate(h.normal*0.1f);
		} else {
			transform.Translate(0, 0, -maxDistance);
		}
	}
}
