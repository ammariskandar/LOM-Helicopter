using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class torque : MonoBehaviour {

	public float torqueAmount;
	Rigidbody rb;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rb.maxAngularVelocity = 1000;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		rb.AddRelativeTorque (new Vector3 (0, torqueAmount, 0));
	}
}
