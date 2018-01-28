using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneTestWallArea : DroneTestArea {

    public GameObject wall;
    public GameObject academy;
    public GameObject goalHolder;
    // Use this for initialization
    void Start () {
        academy = GameObject.Find("Academy");
    }

    // Update is called once per frame
    void Update () {
		
	}

    public override void ResetArea()
    {
        if(!academy.GetComponent<DroneTestWallAcademy>().isInference)
        {
            float goal_y = academy.GetComponent<DroneTestWallAcademy>().goal_y;
            float goal_z = academy.GetComponent<DroneTestWallAcademy>().goal_z;

            goalHolder.transform.position =
            new Vector3(
                Random.Range(-3.5f, 3.5f),
                goal_y,
                goal_z) + this.gameObject.transform.position;
        }
        else
        {
            goalHolder.transform.position =
            new Vector3(
                Random.Range(-3.5f, 3.5f),
                1,
                9) + this.gameObject.transform.position;
        }

    }
}
