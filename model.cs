using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class model : MonoBehaviour {

	public List<joinedPlate> joints = new List<joinedPlate>();


	public void AddJoint(joinedPlate newJoint){
		if (joints == null)
			joints = new List<joinedPlate> ();

		joints.Add (newJoint);
		newJoint.otherPlate.transform.SetParent (transform);
		newJoint.originalPlate.transform.SetParent (transform);
	}

	public bool AlreadyJoined(joinedPlate checkJoint){

		for (int i = 0; i < joints.Count; i ++){
			if (Object.Equals (joints [i].originalPlate, checkJoint.otherPlate) || Object.Equals (joints [i].otherPlate, checkJoint.originalPlate))
				return true;
		}
		return false;
	}
		
}
