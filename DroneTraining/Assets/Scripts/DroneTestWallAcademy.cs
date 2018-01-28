using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneTestWallAcademy : Academy {

    public float goal_y;
    public float goal_z;

    public override void AcademyReset()
    {
        goal_y = (float)resetParameters["goal_y"];
        goal_z = (float)resetParameters["goal_z"];
    }

    public override void AcademyStep()
    {

    }
}
