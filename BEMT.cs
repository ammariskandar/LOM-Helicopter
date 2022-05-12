using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BEMT : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////////////
    //               BEMT Implementation (METHOD B), By Ammar Iskandar                    //
    ////////////////////////////////////////////////////////////////////////////////////////
    //Credits to D.J. Auld & Srinivas for the Matlab code which helped me build this code //
    //Many thanks to Prof. Will Crowther & Conor Marsh for help with Unity and BET        //
    //You can contact me via GitHub or email: ammar.binnorhisham@student.manchester.ac.uk //
    ////////////////////////////////////////////////////////////////////////////////////////
    
    public int NumberOfElements;
    public float RadiusOfRotor,ttoc;
    //chord length of blade assumed constant with radius
    public  float chord; //(TO BE SET)
    //pitch angle.
    public Pitch CubePitch;
    public SpinScript CubeRPM;
    //max cyclic angle - pitch control used to control rolling
    float cyclic = 0.0f / 180 * Mathf.PI;
    //standard sea level atmospheric density
    static float rho = 1.225f;
    //thrust and torque decleration
    [HideInInspector]
    public float thrust,torque,thickness;
    //climb speed
    public float Vc;

    // Start is called before the first frame update
    void Start()
    {
        //Initial Stage
        
        float Pitch = CubePitch.PitchAngle;
        //Rotor Speed
        float RPM = CubeRPM.rpm;
        //collective angle - pitch control used to control vertical rise and descent
        float collective = Pitch/180 * Mathf.PI;
        //radius of the rotor
        float R = RadiusOfRotor;
        //thickness for propeller section (constant with radius for simplicity)
        thickness = ttoc * chord;
        //RPM --> RPS (revs per sec, to be used for omega)
        float n = RPM / 60.0f;
        //rps --> rads per sec
        float omega = n * 2.0f * Mathf.PI;
        //blade segments(starting at 10% R (hub) to 95%R) <--- 10% visually seemed appopriate. Any other values made the hub look weird.
        float rstep = (0.95f - 0.1f) / NumberOfElements * R;
        //forward velocity; set to zero for now, but in future we can use the object's velocity property
        float V = 0.0f;
        //tilt; set to zero for now, but in future we can use the object's transform rotation property
        float tilt = 0.0f / 180.0f * Mathf.PI;

        //max flapping velocity (set to zero because I couldn't really figure out how to implement this or how useful it is, someone can improve this?)
        float vflap = 0.0f;
        thrust = 0.0f;
        torque = 0.0f;
        float Mx = 0.0f;
        float My = 0.0f;
        
        // loop over each blade element (loop i)
        float[] r1 = new float[NumberOfElements+1];
        float[] t1 = new float[NumberOfElements+1];
        float[,] val = new float[NumberOfElements + 1, NumberOfElements + 1];

        // guess initial value of induced velocity (any value works, but 10 works well. Don't go too high or it'll diverge.)
        float Vi = 10.0f;

        for (int i = 1; i <= NumberOfElements; i++)
        {
            float rad = ((0.95f - 0.1f) / NumberOfElements* i + 0.1f) * R; // from 10% (hub) to 95%
            r1[i] = rad / R;
            // loop over each angular sector (loop j)
            for (int j = 1; j <= NumberOfElements; j++)
            {
                float V0=0;
                float V2=0;
                float phi=0;
                float alpha=0;
                float cl=0;
                float cd=0;
                float Vlocal=0;
                float DtDr=0;
                float DqDr=0;
                float tem1=0;
                float Vinew=0;
                float psi = Mathf.PI / (NumberOfElements/2) * j - Mathf.PI / NumberOfElements;
                t1[j] = psi;
                // calculate the local blade element setting angle (useful if you enable cyclic controls. My helicopter right now only flies up and down)
                float theta = collective + cyclic * Mathf.Cos(psi);
                float sigma = 2.0f * chord / 2.0f / Mathf.PI / rad;
                // set logical variable to control iteration (for the while loop)
                bool finished = false;
                // set iteration count and check flag
                int sum = 1;
                int itcheck = 0; //If nothing catastrophic happens, this value should remain 0
                //
                //
                while (!finished)
                {
                    // normal velocity components
                    V0 = Vi + Vc + V * Mathf.Sin(tilt) + vflap * rad * Mathf.Sin(psi); //psi is the current angular position
                    // disk plane velocity
                    V2 = omega * rad + V * Mathf.Cos(tilt) * Mathf.Sin(psi);
                    // flow angle
                    phi = Mathf.Atan2(V0, V2);
                    // blade angle of attack
                    alpha = theta - phi;
                    // lift coefficient
                    cl = 6.2f * alpha;
                    //drag coefficient
                    cd = 0.008f - 0.003f * cl + 0.01f * cl * cl;
                    // local velocity at blade
                    Vlocal = Mathf.Sqrt(V0 * V0 + V2 * V2);
                    // thrust grading
                    DtDr = 0.5f * rho * Vlocal * Vlocal * 2.0f * chord * (cl * Mathf.Cos(phi) - cd * Mathf.Sin(phi)) / NumberOfElements;
                    // torque grading
                    DqDr = 0.5f * rho * Vlocal * Vlocal * 2.0f * chord * rad * (cd * Mathf.Cos(phi) + cl * Mathf.Sin(phi)) / NumberOfElements;
                    // momentum check on induced velocity
                    tem1 = DtDr / (Mathf.PI / 4.0f * rad * rho * V0);
                    // stabilise iteration
                    Vinew = 0.9f * Vi + 0.1f * tem1;
                    //
                    if (Vinew < 0)
                    {
                        Vinew = 0;
                    }
                     // check for convergence
                    if (Mathf.Abs(Vinew - Vi) < 1.0e-5)
                    {
                        finished = true;
                    }
                    Vi = Vinew;
                    // increment iteration count
                    sum = sum + 1;
                    // check to see if the iteration is stuck
                    if (sum > 500)
                    {
                        finished = true; 
                        itcheck = 1;
                    }
                }
                if (itcheck==1){
                    Debug.Log("Error: The iteration was stuck!"); //I've tested changing so many things, and not once did the iteration get stuck, so this is just a failsafe
                }
                val[i, j] = alpha;
                thrust = thrust + DtDr * rstep;
                torque = torque + DqDr * rstep;
                Mx = Mx + rad * Mathf.Sin(psi) * DtDr * rstep;
                My = My + rad * Mathf.Cos(psi) * DtDr * rstep;
            }
        }
        Debug.Log("Thrust: " + thrust);
        Debug.Log("Torque: " + torque);
        Debug.Log("Induced Velocity: " + Vi);
        Debug.Log("Run Time: " + Time.deltaTime);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
