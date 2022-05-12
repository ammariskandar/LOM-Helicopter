 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[System.Serializable]
public class plateMesh : MonoBehaviour {

	[Range(-80,80)]
	public float sweepAngle;

	[Range(0.04f, 0.25f)]
	public float thicknessToChordRatio = 0.12f;

	[Range(0,0.3f)]
	public float controlFlapChordRatio = 0.15f;

	[Range(-15,15)]
	public float controlFlapDeflection = 0;

	[HideInInspector]
	public List<Vector3> verts;

	[HideInInspector]
	public Mesh mesh;

	[HideInInspector]
	public MeshFilter meshFilter;

	[HideInInspector]
	public MeshCollider meshCollider;

	[HideInInspector]
	public Rigidbody rigidBody;


	public int flip = 1;

	public float leftChord = 1;
	public float rightChord = 1;
	public float width = 1;

	[HideInInspector]
	public List<joinedPlate> joinedPlates;

	// MAC dimensions and points
	Vector2 rootPointOne, rootPointTwo;
	Vector2 tipPointOne, tipPointTwo;

	[HideInInspector]
	public Vector2 macPoint;

	[HideInInspector]
	public float macLength;

	[HideInInspector]
	public float macHeight;

	[HideInInspector]
	public float rootHeight;

	[HideInInspector]
	public float tipHeight;



	void calculateMAC(){
		
		float distHolder = 0.5f * leftChord + rightChord;

		Vector2 rootPointOne = new Vector2 (distHolder, -0.5f * flip * width);
		Vector2 rootPointTwo = new Vector2 (-distHolder, -0.5f * flip * width);

		float sweepDist = width * Mathf.Tan (sweepAngle * Mathf.Deg2Rad);
		distHolder = 0.5f * rightChord + leftChord;

		Vector2 tipPointOne = new Vector2 (distHolder + sweepDist, 0.5f * flip * width);
		Vector2 tipPointTwo = new Vector2 (-distHolder + sweepDist, 0.5f * flip * width);

		///	------------------------------------------------------------------------------------------------
		/// 
		/// Equations of lines:
		/// -------------------
		/// 
		///	Using line equations of A1x + B1y = C1 and A2x + B2y = C2
		/// 
		///	Define line 1 as line from rootPointOne to tipPointTwo
		///	Define line 2 as line from rootPointTwo to tipPointOne

		float A1 = tipPointTwo.y - rootPointOne.y;
		float B1 = rootPointOne.x - tipPointTwo.x;
		float C1 = (A1 * rootPointOne.x) + (B1 * tipPointTwo.y);

		float A2 = tipPointOne.y - rootPointTwo.y;
		float B2 = rootPointTwo.x - tipPointOne.x;
		float C2 = (A2 * rootPointTwo.x) + (B2 * tipPointOne.y);

		float det = (A1 * B2) - (A2 * B1);



		if (det == 0) {
			// this means the lines are somehow parallel, should never happen but just in case
			macPoint = Vector2.zero;
			Debug.Log ("Something went wrong");
		} else {
			macPoint = new Vector2 (
				( (B2 * C1) - (B1 * C2) ) / det,					// x coord
				( (A1 * C2) - (A2 * C1) ) / det - flip * width		// y coord
			);
		}

		macLength = lengthDueToTaper (leftChord, rightChord);
		macHeight = lengthDueToTaper (leftChord * thicknessToChordRatio, rightChord * thicknessToChordRatio);



	}

	float lengthDueToTaper(float root, float tip){
		float taperRatio = tip / root;
		float val = root * (2f / 3f) * ((1 + taperRatio + (taperRatio * taperRatio)) / (1 + taperRatio));
		return val;
	}




	////////////////////////////////
	/// AERODYNAMICS
	////////////////////////////////

	//public bool is3D;

	public static Vector3 wind = new Vector3(0,0,0);
	// Plate dimensions
	[HideInInspector]
	public float chord;
	[HideInInspector]
	public float thickness;

	public Vector3 centreOM;
	Vector3 ElevAngle;			// used for setting elevator angle
	float elevAngleDeg;
	Vector3 elevDimensions;
	float plateArea;
	float quarterChord;
	float halfRho;
	// float halfChord;
	float frontArea;
	float MACxPos;
	float MACzPos;

	Vector3 aerodynamicForceWorldAxes;
	Vector3 CPlocationWorld;
	Vector3 aerodynamicForceLocalAxes;
	Vector3 localWindVelocity;
	Vector3 magnus;
	float pitchTorque;

	float alpha;
	float cosSweep;
	float sinSweep;
	// Coefficients and constants
	float Ca;
	public float CnMax = 1.2f;
	float CnMaxS;
	float CaS;
	public float rho = 1.3f;
	float dampConst;
	// float magnusConst;

	public bool plate3d = false;

	// Component containers
	Rigidbody plateRB;
	Transform elevRot;



	public void getDimensions(){

		calculateMAC ();

		chord = macLength;
		thickness = thicknessToChordRatio * chord;

		cosSweep = Mathf.Cos(-sweepAngle * Mathf.Deg2Rad);
		sinSweep = (float)flip * Mathf.Sin(sweepAngle *Mathf.Deg2Rad);

		MACxPos = macPoint.y;
		MACzPos = macPoint.x;

		plateRB = GetComponent<Rigidbody>();

		// Constants
		plateArea = chord * width;
		quarterChord = 0.25f * chord;
		halfRho = 0.5f * rho;
		// halfChord = chord / 2f;
		frontArea = width * thickness;
		Ca = 0.01f + (thicknessToChordRatio * CnMax);
		CaS = Ca * frontArea;
		dampConst = -rho * 2f * 0.25f * Mathf.Pow ((chord / 2), 4) * width;
		CnMaxS = CnMax * plateArea;
		// magnusConst = rho * 2f * Mathf.PI * halfChord * width * 2 * thickness/(Mathf.PI*chord);

		plateRB.maxAngularVelocity = 50; // put this up to 50 because its default is something like 7

	}

	void Start () {
		getDimensions ();
	}


	/* 2D VERSION THAT WORKS
	void FixedUpdate () {

		// Get wind velocity from wind and plate velocity
		velocityWind = transform.InverseTransformDirection(-plateRB.velocity) + transform.InverseTransformDirection(wind);// Inverse transform the wind and plate velocity
		velocityWind.z *= cosSweep; // Accounting for the plate's sweep angle

		alpha = Mathf.Atan2 (velocityWind.y, -velocityWind.z);							// Calculate angle of attack from local wind velocity vector
		float vSquared = velocityWind.z*velocityWind.z + velocityWind.y*velocityWind.y;	// Relative wind speeed squared, don't use wind velocity in element spanwise direction
																						// so Vector3.magnitude is inefficient

		float cosAlpha = Mathf.Cos(alpha);	// Store value to prevent recalculation
		float sinAlpha = Mathf.Sin (alpha);	// Store value to prevent recalculation

		// Aerodynamic Centre
		float CPz = (quarterChord * cosAlpha) + MACzPos;		// CP position = 0.25 * c * cos(alpha)
		Vector3 CPlocation = new Vector3 (MACxPos, 0, CPz); 	// Set the x position of the AC position vector accordingly
		CPlocationWorld = transform.TransformPoint(CPlocation);	// Scale the location to world coordinates

		// Coefficients
		float Cn = CnMaxS * sinAlpha;							// Normal coefficient at current angle of attack
		float dPressure = halfRho * vSquared;					// Dynamic pressure = 0.5 * rho * VSquared
		float CMo = 0.0167f * controlFlapDeflection ;			// Cmo due to the camber effect of flap deflection

		//pitchDamp due to rotaional velocity
		Vector3 localAngularVel = transform.InverseTransformDirection(plateRB.angularVelocity);	// Get angular velocity in local coords
		float q = localAngularVel.x;															// Store x component of angular velocity ie pitch rate, q
		float pitchDamp =  Mathf.Abs(q) * q * dampConst;										// Pitch damp torque from constant and q

		// Magnus lift - NOT IN USE AT THE MOMENT
		// Vector3 magnus = magnusConst * Vector3.Cross(velocityWind, (halfChord) * localAngularVel);	// Calculate magnus, cross product ensures direction is perpendicular to wind direction
		float ex = halfChord * q;
		//Vector3 magnus = new Vector3(0, -magnusConst * (velocityWind.x * ex), magnusConst * (velocityWind.y * ex));

		// Normal and axial forces
		float normalForce = (Cn * dPressure);				// Normal force from coefficient, area is already multiplied by CnMax in Start
		float axialForce = (dPressure * CaS * cosAlpha);	// Axial force from coefficient, area is already multiplied by Ca in Start

		// Total torque acting on element
		pitchTorque = pitchDamp + (CMo*dPressure*plateArea*chord);

		// Apply torques and forces
		plateRB.AddRelativeTorque(new Vector3(pitchTorque,0,0));										// Pitch torque
		aerodynamicForceLocalAxes = new Vector3 (0, normalForce, -axialForce);					// Normal and axial forces as a local vector
		aerodynamicForceWorldAxes = transform.TransformDirection (aerodynamicForceLocalAxes);	// Convert normal and axial forces to world coords
		//Vector3 magnusForceWorldAxes = plateTrans.TransformDirection (magnus);							// Convert magnus force to world coords
		plateRB.AddForceAtPosition (aerodynamicForceWorldAxes, CPlocationWorld);						// Apply normal and axial forces at CP
		//plateRB.AddForce(magnusForceWorldAxes);															// Apply magnus at geometric centre of element
	}

*/

	// Trying for the 3D version
	void FixedUpdate () {

		// Get wind velocity from wind and plate velocity
		localWindVelocity = transform.InverseTransformDirection (-plateRB.velocity) + transform.InverseTransformDirection (wind);// Inverse transform the wind and plate velocity
		localWindVelocity.z *= cosSweep; // Accounting for the plate's sweep angle
		localWindVelocity.z += localWindVelocity.x * sinSweep;

		alpha = Mathf.Atan2 (localWindVelocity.y, -localWindVelocity.z);							// Calculate angle of attack from local wind velocity vector
		float vSquared = localWindVelocity.z * localWindVelocity.z + localWindVelocity.y * localWindVelocity.y;	// Relative wind speeed squared, don't use wind velocity in element spanwise direction
		// so Vector3.magnitude is inefficient

		float cosAlpha = Mathf.Cos (alpha);	// Store value to prevent recalculation
		float sinAlpha = Mathf.Sin (alpha);	// Store value to prevent recalculation

		// Aerodynamic Centre
		float CPz = (quarterChord * cosAlpha) + MACzPos;		// CP position = 0.25 * c * cos(alpha)
		Vector3 CPlocation = new Vector3 (MACxPos, 0, CPz); 	// Set the x position of the CP position vector accordingly
		CPlocationWorld = transform.TransformPoint (CPlocation);	// Scale the location to world coordinates

		// Coefficients
		float Cn = CnMaxS * sinAlpha;							// Normal coefficient at current angle of attack
		float dPressure = halfRho * vSquared;					// Dynamic pressure = 0.5 * rho * VSquared
		float CMo = 0.0167f * controlFlapDeflection;			// Cmo due to the camber effect of flap deflection

		//pitchDamp due to rotaional velocity
		Vector3 localAngularVel = transform.InverseTransformDirection (plateRB.angularVelocity);	// Get angular velocity in local coords
		float q = localAngularVel.x;															// Store x component of angular velocity ie pitch rate, q
		float pitchDamp = Mathf.Abs (q) * q * dampConst;										// Pitch damp torque from constant and q

		// Magnus lift - NOT IN USE AT THE MOMENT
		//if (plate3d) {
			//Vector3 magnus = magnusConst * Vector3.Cross(velocityWind, (halfChord) * localAngularVel);	// Calculate magnus, cross product ensures direction is perpendicular to wind direction
			//float ex = halfChord * q;
			//magnus = new Vector3 (0, -magnusConst * (localWindVelocity.x * ex), magnusConst * (localWindVelocity.y * ex));
		//
	//}
		// Normal and axial forces
		float normalForce = (Cn * dPressure);				// Normal force from coefficient, area is already multiplied by CnMax in Start
		float axialForce = (dPressure * CaS * cosAlpha);    // Axial force from coefficient, area is already multiplied by Ca in Start


		// Total torque acting on element
		pitchTorque = pitchDamp + (CMo * dPressure * plateArea * chord);

		// Apply torques and forces
		plateRB.AddRelativeTorque (new Vector3 (pitchTorque, 0, 0));							// Pitch torque
		aerodynamicForceLocalAxes = new Vector3 (0, normalForce, -axialForce);					// Normal and axial forces as a local vector
		aerodynamicForceWorldAxes = transform.TransformDirection (aerodynamicForceLocalAxes);	// Convert normal and axial forces to world coords
		plateRB.AddForceAtPosition (aerodynamicForceWorldAxes, CPlocationWorld);				// Apply normal and axial forces at CP

		// Vector3 magnusForceWorldAxes = transform.TransformDirection (magnus);				// Convert magnus force to world coords
		// plateRB.AddForce (magnusForceWorldAxes);											// Apply magnus at geometric centre of element

	}


	public Vector3 mac;
	/*
	void OnDrawGizmos(){
		mac = new Vector3 (macPoint.y, 0, macPoint.x);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere (CPlocationWorld, 0.1f);
		Gizmos.DrawWireSphere(transform.TransformPoint(mac), 0.1f);
	}
*/

}

