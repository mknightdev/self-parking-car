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
    private bool hasStopped = false;
    private bool hasStoppedCheck = false;
    private bool startTimer = false;
    private float timer = 0.0f;

    public List<Transform> targets;

    private void Start()
    {
        // Get the agent's rigidbody
        agentRb = this.GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        GlobalStats.UpdateText();

        // Get target
        //target = this.transform.parent.Find("Target").transform;
        //Debug.Log($"TargetLocal: {target.position}");

        // Get the environment settings
        envSettings = FindObjectOfType<EnvSettings>();

        // Get the floor
        floorRend = this.transform.parent.Find("Floor").GetComponent<MeshRenderer>();
        floorMat = floorRend.material;

        // Find all potential targets
        for (int i = 0; i < this.transform.parent.childCount; i++)
        {
            // Compare name
            if (this.transform.parent.GetChild(i).name == "Target")
            {
                targets.Add(this.transform.parent.GetChild(i));
            }
        }
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

        // Choose random target
        target = targets[Random.Range(0, targets.Count)];
        for (int i = 0; i < targets.Count; i++)
        {
            if (target != targets[i])
            {
                // Hide mesh renderer and box collider 
                targets[i].GetComponent<MeshRenderer>().enabled = false;
                targets[i].GetComponent<BoxCollider>().enabled = false;



                // Show the car
                targets[i].GetChild(0).Find("Car").GetComponent<MeshRenderer>().enabled = true;
                targets[i].GetChild(0).Find("Car").GetComponent<BoxCollider>().enabled = true;
            }
            else
            {
                // Show the target
                target.GetComponent<MeshRenderer>().enabled = true;
                target.GetComponent<BoxCollider>().enabled = true;

                // Hide the car
                target.GetChild(0).Find("Car").GetComponent<MeshRenderer>().enabled = false;
                target.GetChild(0).Find("Car").GetComponent<BoxCollider>().enabled = false;
            }

            // Reset Cars
            targets[i].GetChild(0).Find("Car").GetComponent<NPCManager>().ResetNPC();
        }

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent Pos, Rot
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.transform.localRotation);

        sensor.AddObservation(this.agentRb.velocity.x);
        sensor.AddObservation(this.agentRb.velocity.z);

        // Speed
        sensor.AddObservation(this.moveSpeed);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        agentRb.AddForce(controlSignal * moveSpeed, ForceMode.VelocityChange);

        // Rewards
        float distance = Vector3.Distance(this.transform.localPosition, target.localPosition);
        //Debug.Log($"Distance: {distance}");

        if (distance < 1.5f)
        {
            // If the agent has got closer, reward it
            SetReward(1.0f);
        }
        else
        {
            // If the agent hasn't got closer, punish it
            SetReward(-0.1f);
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
        if (other.CompareTag("target") && !hasStoppedCheck) 
        {
            hasStoppedCheck = true;
            StartCoroutine(HasParked());
        }

        if (other.CompareTag("target") && hasStopped)
        {
            GlobalStats.success += 1;

            SetReward(5.0f);
            EndEpisode();
            hasStopped = false;
            hasStoppedCheck = false;
            StartCoroutine(SwapMaterial(envSettings.winMat, 2.0f));
        }
    }

    IEnumerator HasParked()
    {
        yield return new WaitForSeconds(1.0f);
        hasStopped = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("wall"))
        {
            SetReward(-0.5f);
        }

        if (collision.transform.CompareTag("car"))
        {
            Debug.Log("Collision Enter");
            SetReward(-0.1f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("wall"))
        {
            SetReward(-1.0f);
        }

        if (collision.transform.CompareTag("car"))
        {
            Debug.Log("Collision Stay");
            SetReward(-0.01f);
        }
    }

    IEnumerator SwapMaterial(Material mat, float time)
    {
        floorRend.material = mat;
        yield return new WaitForSeconds(time);  // wait for 2 seconds
        floorRend.material = floorMat;
    }
}