using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class joinedPlate {
	
	public plateMesh otherPlate;
	public plateMesh originalPlate;
	public ConfigurableJoint joint;
	public bool originalPlateJoinedRight;
	public bool otherPlateJoinedRight;

	public int ID;


	public joinedPlate(){
		
	}

	public joinedPlate(plateMesh _plate, plateMesh _originalPlate, ConfigurableJoint _joint){
		otherPlate = _plate;
		originalPlate = _originalPlate;
		joint = _joint;

	}

	public static joinedPlate swapOverPlates(joinedPlate jp){
		
		joinedPlate newJP = new joinedPlate ();

		newJP.originalPlate = jp.otherPlate;
		newJP.otherPlate = jp.originalPlate;
		newJP.otherPlateJoinedRight = jp.originalPlateJoinedRight;
		newJP.originalPlateJoinedRight = jp.otherPlateJoinedRight;
		newJP.joint = jp.joint;
		newJP.ID = jp.ID;

		return newJP;
	}
}
