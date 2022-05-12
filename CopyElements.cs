using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyElements : MonoBehaviour
{
    public int NumberOfElements;
    public int RadiusOfRotor;
    public GameObject Element;
    public GameObject Hub;
    // Start is called before the first frame update
    void Start()
    {
        float hubrad = 0.1f * RadiusOfRotor;
        float r = RadiusOfRotor - hubrad;
        float scale = r / NumberOfElements;
        for (int i = 0; i < NumberOfElements; i++)
        {
            Hub.transform.localScale = new Vector3(hubrad, 1.25f, hubrad); //resize hub to 10% of entire radius
            Hub.transform.position = new Vector3(0, 0, 0); // position hub
            Element.transform.localScale = new Vector3(1, 0.1f, scale); // resize elements to match radius
            GameObject duplicate = Instantiate(Element, new Vector3(0,1.0F,i * scale + hubrad), Quaternion.identity); //i * (1+separation between elements) + (hub offset)

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
