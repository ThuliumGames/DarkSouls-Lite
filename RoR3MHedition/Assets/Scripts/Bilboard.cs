using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bilboard : MonoBehaviour {
	
	void Update () {
		transform.localScale += Vector3.one*0.04f;
		transform.position += Vector3.up*0.02f;
		transform.LookAt(Camera.main.transform.position);
	}
}
