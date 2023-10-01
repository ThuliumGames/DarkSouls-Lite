using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillObject : MonoBehaviour {
	
	public float i=1f;
	
	public bool doSlow, targetEnemy;
	
	GameObject[] enemies;
	Rigidbody rb;
	
	void Start () {
		if (targetEnemy) {
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			rb = GetComponent<Rigidbody>();
		}
	}
	
	void Update () {
		if (i>0.25f && doSlow) {
			Globals.animationSpeed = 1f;
		}
		
		i-=0.05f;
		if (i<0) {
			Destroy(gameObject);
		}
		
		if (i>0.25f && doSlow) {
			Globals.animationSpeed = 0.15f;
		}
	}
	
	void FixedUpdate () {
		if (targetEnemy && enemies.Length>0) {
			float dist = 20;
			Transform e = null;
			for(int x=0; x<enemies.Length; x++) {
				if (enemies[x]!=null) {
					if (Vector3.Distance(transform.position, enemies[x].transform.position)<dist&&Vector3.Dot(rb.velocity.normalized, (enemies[x].transform.position-transform.position).normalized)>0.9f) {
						dist = Vector3.Distance(transform.position, enemies[x].transform.position);
						e = enemies[x].transform;
					}
				}
			}
			if (e!=null) {
				rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(((e.position-transform.position).normalized*20).x, rb.velocity.y, ((e.position-transform.position).normalized*20).z), 0.04f);
			}
		}
	}
	
	void OnDestroy () {
		if (doSlow) {
			Globals.animationSpeed = 1;
		}
	}
}
