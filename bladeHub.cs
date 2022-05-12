using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bladeHub : MonoBehaviour {

	public int numberOfBlades;
	public plateMesh plate;
	[HideInInspector()]
	public List<plateMesh> plates;
	public float distanceFromHub;



	void Start(){
		GetComponentInChildren<Rigidbody> ().maxAngularVelocity = 50f;
	}

}
