using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class thrust : MonoBehaviour {

	public Vector3 thrustForce;
	Rigidbody rb;

	void Start(){
		rb = GetComponent<Rigidbody> ();
		rb.AddRelativeForce (thrustForce, ForceMode.Impulse);
	}
	// Update is called once per frame
	void FixedUpdate () {
		rb.AddRelativeForce (thrustForce);
	}
}
