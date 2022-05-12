using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lift : MonoBehaviour
{
    public BEMT BEMTCode;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<ConstantForce>().force = new Vector3(0f, BEMTCode.thrust, 0);
    }
}
