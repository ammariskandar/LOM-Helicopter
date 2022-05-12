using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinScript : MonoBehaviour
{
    public float rpm;
    float degpersec;

    // Start is called before the first frame update
    void Start()
    {
        degpersec = rpm * 6;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.RotateAround(Vector3.zero, Vector3.up, degpersec * Time.deltaTime);
    }
}
