using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour {
	
	public static float animationSpeed=1;
	
	public static Transform player;
	
	void Start () {
		animationSpeed = 1;
		player = GameObject.FindGameObjectWithTag("Player").transform;
	}
}
