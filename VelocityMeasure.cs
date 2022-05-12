using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityMeasure : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 vel = rb.velocity;
        float velmag = vel.magnitude;
        Debug.Log(velmag);
    }
}
