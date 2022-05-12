using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubScale : MonoBehaviour
{
    public BEMT BEMTCode;
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(BEMTCode.RadiusOfRotor*0.1f, 1, BEMTCode.RadiusOfRotor*0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
