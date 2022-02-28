using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    public EnvSettings envSettings;

    public float moveSpeed;
    public Transform target;

    private Rigidbody agentRb;

    private MeshRenderer floorRend;
    private Material floorMat;
    private float oldDistance = 0.0f;

    private void Start()
    {
        // Get the agent's rigidbody
        agentRb = GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        GlobalStats.UpdateText();

        // Get target
        target = this.transform.parent.Find("ParkingSpot").Find("Target").transform;
        //Debug.Log($"TargetLocal: {target.position}");

        // Get the environment settings
        envSettings = FindObjectOfType<EnvSettings>();

        // Get the floor
        floorRend = this.transform.parent.Find("Floor").GetComponent<MeshRenderer>();
        floorMat = floorRend.material;
    }

    public override void OnEpisodeBegin()
    {
        GlobalStats.episode += 1;

        // Move agent back to starting position
        this.transform.localPosition = new Vector3(0.0f, 0.0f, -8.0f);
        this.transform.localRotation = Quaternion.identity;

        // Zero the velocity
        this.agentRb.velocity = Vector3.zero;
        this.agentRb.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent Pos, Rot
        sensor.AddObservation(target.position);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.transform.localRotation);

        sensor.AddObservation(this.agentRb.velocity.x);
        sensor.AddObservation(this.agentRb.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        agentRb.AddForce(controlSignal * moveSpeed, ForceMode.VelocityChange);

        // Rewards
        float distance = Vector3.Distance(this.transform.localPosition, target.position);
        //Debug.Log($"Distance: {distance}");

        if (distance < oldDistance)
        {
            // If the agent has got closer, reward it
            SetReward(1f);
            oldDistance = distance;
        }
        else
        {
            // If the agent hasn't got closer, punish it
            SetReward(-0.1f);
            oldDistance = distance;
        }

        // Punish if it falls off the platform
        if (this.transform.localPosition.y < -1.0f)
        {
            GlobalStats.fail += 1;

            SetReward(-0.25f);
            EndEpisode();
            StartCoroutine(SwapMaterial(envSettings.failMat, 2.0f));
        }

        // Stats
        GlobalStats.UpdateText();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continousActionsOut = actionsOut.ContinuousActions;
        continousActionsOut[0] = Input.GetAxis("Horizontal");
        continousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("target"))
        {
            GlobalStats.success += 1;

            SetReward(5f);
            EndEpisode();
            StartCoroutine(SwapMaterial(envSettings.winMat, 2.0f));
        }
    }

    IEnumerator SwapMaterial(Material mat, float time)
    {
        floorRend.material = mat;
        yield return new WaitForSeconds(time);  // wait for 2 seconds
        floorRend.material = floorMat;
    }
}
