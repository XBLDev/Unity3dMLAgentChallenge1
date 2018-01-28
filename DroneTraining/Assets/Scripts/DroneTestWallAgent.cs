using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneTestWallAgent : DroneTestAreaAgent
{
    public GameObject goalHolder;
    public GameObject wall;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

    public override List<float> CollectState()
    {
        List<float> state = new List<float>();
        Vector3 velocity = GetComponent<Rigidbody>().velocity;

        state.Add((transform.position.x - area.transform.position.x));
        state.Add((transform.position.y - area.transform.position.y));
        state.Add((transform.position.z - area.transform.position.z));

        state.Add((goalHolder.transform.position.x - area.transform.position.x));
        state.Add((goalHolder.transform.position.y - area.transform.position.y));
        state.Add((goalHolder.transform.position.z - area.transform.position.z));

        state.Add(velocity.x);
        state.Add(velocity.y);
        state.Add(velocity.z);

        return state;
    }

    public override void AgentStep(float[] act)
    {
        reward = -0.005f;
        MoveAgent(act);

        if (gameObject.transform.position.y < 0.0f)
        {
            done = true;
            reward = -1f;
        }
    }

    public override void AgentOnDone()
    {
    }

}
