using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scale : MonoBehaviour
{
    public BEMT BEMTCode;
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(BEMTCode.chord, BEMTCode.ttoc* BEMTCode.chord, BEMTCode.RadiusOfRotor*2);
        transform.position = new Vector3(0,1,0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
