using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;

public class RollerAgent : Agent
{

    public float forceMultiplier = 10;
    public Transform target;
    private Rigidbody rb;

    public RollerSettings rollerSettings;
    public MeshRenderer groundRenderer;
    public Material groundMaterial;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    public override void Initialize()
    {
        rollerSettings = FindObjectOfType<RollerSettings>();
        groundMaterial = groundRenderer.material;
    }

    public override void OnEpisodeBegin()
    {
        GlobalStats.episode += 1;

        //cumalativeRewardCountText.text = $"{this.cumalativeRewardCount}";
        //stepCountText.text = $"{this.stepCount}";

        // If the agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rb.angularVelocity = Vector3.zero;
            this.rb.velocity = Vector3.zero;
        }

        // Move agent back to starting position
        this.transform.localPosition = new Vector3(0, 0f, -8f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        rb.AddForce(controlSignal * forceMultiplier);
        
        // Rewards
        float distance = Vector3.Distance(this.transform.localPosition, target.localPosition);
        Debug.Log($"Distance: {distance}");
        
        // Target reached
        if (distance < 1.42f)
        {
            GlobalStats.success += 1;

            SetReward(1f);
            EndEpisode();
            StartCoroutine(SwapMaterial(rollerSettings.winMat, 2.0f));
        }
        else if (this.transform.localPosition.y < 0)
        {
            GlobalStats.fail += 1;

            // Fell off platform
            SetReward(-0.25f);
            EndEpisode();
            StartCoroutine(SwapMaterial(rollerSettings.failMat, 2.0f));
        }

        GlobalStats.UpdateText();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("obstacle"))
        {
            GlobalStats.fail += 1;

            SetReward(-0.50f);
            EndEpisode();
            StartCoroutine(SwapMaterial(rollerSettings.failMat, 2.0f));
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    IEnumerator SwapMaterial(Material mat, float time)
    {
        groundRenderer.material = mat;
        yield return new WaitForSeconds(time);  // wait for 2 seconds
        groundRenderer.material = groundMaterial;
    }
}
