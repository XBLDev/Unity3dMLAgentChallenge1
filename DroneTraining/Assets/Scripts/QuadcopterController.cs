﻿using UnityEngine;
using System.Collections;

public class QuadcopterController : MonoBehaviour
{
    //The propellers
    public GameObject propellerFR;
    public GameObject propellerFL;
    public GameObject propellerBL;
    public GameObject propellerBR;

    //Quadcopter parameters
    [Header("Internal")]
    public float maxPropellerForce; //100
    public float maxTorque; //1
    public float throttle;
    public float moveFactor; //5
    //PID
    public Vector3 PID_pitch_gains; //(2, 3, 2)
    public Vector3 PID_roll_gains; //(2, 0.2, 0.5)
    public Vector3 PID_yaw_gains; //(1, 0, 0)


    //External parameters
    [Header("External")]
    public float windForce;
    //0 -> 360
    public float forceDir;


    Rigidbody quadcopterRB;


    //The PID controllers
    private PIDController PID_pitch;
    private PIDController PID_roll;
    private PIDController PID_yaw;

    //Movement factors
    float moveForwardBack;
    float moveLeftRight;
    float yawDir;

    void Start()
    {
        quadcopterRB = gameObject.GetComponent<Rigidbody>();

        PID_pitch = new PIDController();
        PID_roll = new PIDController();
        PID_yaw = new PIDController();
    }

    void FixedUpdate()
    {
        AddControls();

        AddMotorForce();

        //AddExternalForces();
    }


    float forceApplied_FR = 0f;
    float forceApplied_FL = 0f;
    float forceApplied_BR = 0f;
    float forceApplied_BL = 0f;

    float rotation_FR = 0f;
    float rotation_FL = 0f;
    float rotation_BR = 0f;
    float rotation_BL = 0f;

    bool addingForce_FR = false;
    bool addingForce_FL = false;
    bool addingForce_BR = false;
    bool addingForce_BL = false;

    public bool autoFloating = false;
    //public bool useNewControl = true;
    //public bool useOldControl = false;

    void addForceToOnePropeller(GameObject onepropeller)
    {
        Vector3 propellerUp = onepropeller.transform.up;
        Vector3 propellerPos = onepropeller.transform.position;
        quadcopterRB.AddForceAtPosition(propellerUp * 90f, propellerPos);
        if(onepropeller == propellerFR)
        {
            rotation_FR += 5f;
            rotation_FR = Mathf.Clamp(rotation_FR, 0f, 30f);
            addingForce_FR = true;
        }
        if (onepropeller == propellerFL)
        {
            rotation_FL += 5f;
            rotation_FL = Mathf.Clamp(rotation_FL, 0f, 30f);
            addingForce_FL = true;
        }

        if (onepropeller == propellerBR)
        {
            rotation_BR += 5f;
            rotation_BR = Mathf.Clamp(rotation_BR, 0f, 30f);
            addingForce_BR = true;
        }
        if (onepropeller == propellerBL)
        {
            rotation_BL += 5f;
            rotation_BL = Mathf.Clamp(rotation_BL, 0f, 30f);
            addingForce_BL = true;
        }
    }

    void AddControls()
    {

        addingForce_FR = false;
        addingForce_FL = false;
        addingForce_BR = false;
        addingForce_BL = false;

        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("keypad5 pressed");//LB propeller
            addForceToOnePropeller(propellerFR);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            Debug.Log("keypad4 pressed");//RB propeller
            addForceToOnePropeller(propellerFL);
        }

        if (Input.GetKey(KeyCode.S))
        {
            Debug.Log("keypad2 pressed");//LB propeller
            addForceToOnePropeller(propellerBR);
        }
        if (Input.GetKey(KeyCode.A))
        {
            Debug.Log("keypad1 pressed");//RB propeller
            addForceToOnePropeller(propellerBL);
        }

        if (autoFloating)
        {
            if (gameObject.transform.position.y < 1.5f)
            {
                addForceToOnePropeller(propellerFR);
                addForceToOnePropeller(propellerFL);
                addForceToOnePropeller(propellerBR);
                addForceToOnePropeller(propellerBL);
            }
        }

        if (!addingForce_FR)
        {
            rotation_FR -= 0.5f;
            rotation_FR = Mathf.Clamp(rotation_FR, 0f, 30f);
        }
        if (!addingForce_FL)
        {
            rotation_FL -= 0.5f;
            rotation_FL = Mathf.Clamp(rotation_FL, 0f, 30f);
        }
        if (!addingForce_BR)
        {
            rotation_BR -= 0.5f;
            rotation_BR = Mathf.Clamp(rotation_BR, 0f, 30f);
        }
        if (!addingForce_BL)
        {
            rotation_BL -= 0.5f;
            rotation_BL = Mathf.Clamp(rotation_BL, 0f, 30f);
        }

        propellerFR.gameObject.transform.Rotate(new Vector3(0, rotation_FR, 0));
        propellerFL.gameObject.transform.Rotate(new Vector3(0, rotation_FL, 0));
        propellerBR.gameObject.transform.Rotate(new Vector3(0, rotation_BR, 0));
        propellerBL.gameObject.transform.Rotate(new Vector3(0, rotation_BL, 0));

    }

    void AddMotorForce()
    {
        //Calculate the errors so we can use a PID controller to stabilize
        //Assume no error is if 0 degrees

        //Pitch
        //Returns positive if pitching forward
        float pitchError = GetPitchError();

        //Roll
        //Returns positive if rolling left
        float rollError = GetRollError() * -1f;

        //Adapt the PID variables to the throttle
        Vector3 PID_pitch_gains_adapted = throttle > 100f ? PID_pitch_gains * 2f : PID_pitch_gains;

        //Get the output from the PID controllers
        float PID_pitch_output = PID_pitch.GetFactorFromPIDController(PID_pitch_gains_adapted, pitchError);
        float PID_roll_output = PID_roll.GetFactorFromPIDController(PID_roll_gains, rollError);

        //Calculate the propeller forces
        //FR
        float propellerForceFR = throttle + (PID_pitch_output + PID_roll_output);

        //Add steering
        propellerForceFR -= moveForwardBack * throttle * moveFactor;
        propellerForceFR -= moveLeftRight * throttle;


        //FL
        float propellerForceFL = throttle + (PID_pitch_output - PID_roll_output);

        propellerForceFL -= moveForwardBack * throttle * moveFactor;
        propellerForceFL += moveLeftRight * throttle;


        //BR
        float propellerForceBR = throttle + (-PID_pitch_output + PID_roll_output);

        propellerForceBR += moveForwardBack * throttle * moveFactor;
        propellerForceBR -= moveLeftRight * throttle;


        //BL 
        float propellerForceBL = throttle + (-PID_pitch_output - PID_roll_output);

        propellerForceBL += moveForwardBack * throttle * moveFactor;
        propellerForceBL += moveLeftRight * throttle;


        //Clamp
        propellerForceFR = Mathf.Clamp(propellerForceFR, 0f, maxPropellerForce);
        //Debug.Log("FR Force: " + propellerForceFR);
        propellerForceFL = Mathf.Clamp(propellerForceFL, 0f, maxPropellerForce);
        propellerForceBR = Mathf.Clamp(propellerForceBR, 0f, maxPropellerForce);
        propellerForceBL = Mathf.Clamp(propellerForceBL, 0f, maxPropellerForce);

        //Add the force to the propellers
        AddForceToPropeller(propellerFR, propellerForceFR);
        AddForceToPropeller(propellerFL, propellerForceFL);
        AddForceToPropeller(propellerBR, propellerForceBR);
        AddForceToPropeller(propellerBL, propellerForceBL);

        //Yaw
        //Minimize the yaw error (which is already signed):
        float yawError = quadcopterRB.angularVelocity.y;

        float PID_yaw_output = PID_yaw.GetFactorFromPIDController(PID_yaw_gains, yawError);

        //First we need to add a force (if any)
        quadcopterRB.AddTorque(transform.up * yawDir * maxTorque * throttle);

        //Then we need to minimize the error
        quadcopterRB.AddTorque(transform.up * throttle * PID_yaw_output * -1f);
    }

    void AddForceToPropeller(GameObject propellerObj, float propellerForce)
    {
        Vector3 propellerUp = propellerObj.transform.up;

        Vector3 propellerPos = propellerObj.transform.position;

        quadcopterRB.AddForceAtPosition(propellerUp * propellerForce, propellerPos);

        //Debug
        //Debug.DrawRay(propellerPos, propellerUp * 1f, Color.red);
    }

    //Pitch is rotation around x-axis
    //Returns positive if pitching forward
    private float GetPitchError()
    {
        float xAngle = transform.eulerAngles.x;

        //Make sure the angle is between 0 and 360
        xAngle = WrapAngle(xAngle);

        //This angle going from 0 -> 360 when pitching forward
        //So if angle is > 180 then it should move from 0 to 180 if pitching back
        if (xAngle > 180f && xAngle < 360f)
        {
            xAngle = 360f - xAngle;

            //-1 so we know if we are pitching back or forward
            xAngle *= -1f;
        }

        return xAngle;
    }

    //Roll is rotation around z-axis
    //Returns positive if rolling left
    private float GetRollError()
    {
        float zAngle = transform.eulerAngles.z;

        //Make sure the angle is between 0 and 360
        zAngle = WrapAngle(zAngle);

        //This angle going from 0-> 360 when rolling left
        //So if angle is > 180 then it should move from 0 to 180 if rolling right
        if (zAngle > 180f && zAngle < 360f)
        {
            zAngle = 360f - zAngle;

            //-1 so we know if we are rolling left or right
            zAngle *= -1f;
        }

        return zAngle;
    }

    //Wrap between 0 and 360 degrees
    float WrapAngle(float inputAngle)
    {
        //The inner % 360 restricts everything to +/- 360
        //+360 moves negative values to the positive range, and positive ones to > 360
        //the final % 360 caps everything to 0...360
        return ((inputAngle % 360f) + 360f) % 360f;
    }

    //Add external forces to the quadcopter, such as wind
    private void AddExternalForces()
    {
        //Important to not use the quadcopters forward
        Vector3 windDir = -Vector3.forward;

        //Rotate it 
        windDir = Quaternion.Euler(0, forceDir, 0) * windDir;

        quadcopterRB.AddForce(windDir * windForce);

        //Debug
        //Is showing in which direction the wind is coming from
        //center of quadcopter is where it ends and is blowing in the direction of the line
        Debug.DrawRay(transform.position, -windDir * 3f, Color.red);
    }
}
