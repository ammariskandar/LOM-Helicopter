using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitch : MonoBehaviour
{
    public int PitchAngle; //element pitch angle (user defined in editor)
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.AngleAxis(PitchAngle, Vector3.forward);
    }
}
