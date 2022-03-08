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
    public List<Transform> targets;

    private Rigidbody agentRb;

    private MeshRenderer floorRend;
    private Material floorMat;
    private float disFromGoal = 0.0f;
    private Vector3 defaultPos;

    private NPCManager[] npcManagers;

    private void Start()
    {
        // Get the agent's rigidbody
        agentRb = GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        GlobalStats.UpdateText();

        // Get target
        //target = this.transform.parent.Find("Target").transform;

        // Get target
        //target = targets[Random.Range(0, targets.Count)];
        //target.Find("ParkingSpot").Find("Car").gameObject.SetActive(false); // Hide car


        //for (int i = 0; i < targets.Count; i++)
        //{
        //    // Hide other targets
        //    if (target != targets[i])
        //    {
        //        // Hide the target mesh and collider
        //        targets[i].GetComponent<BoxCollider>().enabled = false;
        //        targets[i].GetComponent<MeshRenderer>().enabled = false;
        //    }
        //}

        //Debug.Log($"TargetLocal: {target.position}");

        // Get the environment settings
        envSettings = FindObjectOfType<EnvSettings>();

        // Get the floor
        floorRend = this.transform.parent.Find("Floor").GetComponent<MeshRenderer>();
        floorMat = floorRend.material;

        npcManagers = FindObjectsOfType<NPCManager>();
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

        // Reset NPC Car
        for (int i = 0; i < npcManagers.Length; i++)
        {
            npcManagers[i].ResetNPC();
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
        // Moving
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        agentRb.AddForce(controlSignal * moveSpeed, ForceMode.VelocityChange);

        float lastDistance = Vector3.Distance(this.transform.localPosition, target.localPosition);

        // Rewards
        disFromGoal = Vector3.Distance(this.transform.localPosition, target.localPosition);

        if (disFromGoal < lastDistance)
        {
            SetReward(0.01f);
        }


        float disFromGoalX = Mathf.Abs(this.transform.localPosition.x - target.localPosition.x);
        float disFromGoalZ = Mathf.Abs(this.transform.localPosition.z - target.localPosition.z);
        Debug.Log($"Distance X: {disFromGoalX}");
        Debug.Log($"Distance Z: {disFromGoalZ}");



        float threshold = 5f;
        if (disFromGoalZ < threshold)
        {
            // If the agent has got closer, reward it
            SetReward(1.0f);
        }

        if (disFromGoalX < 0.15f && disFromGoalZ < 2.5f)
        {
            SetReward(1.0f);
        }

        // Punish for being outside of 2.5 
        if (disFromGoalX > 2.5f)
        {
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
        if (other.CompareTag("target"))
        {
            GlobalStats.success += 1;

            SetReward(5.0f);
            EndEpisode();
            StartCoroutine(SwapMaterial(envSettings.winMat, 2.0f));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("wall"))
        {
            SetReward(-0.5f);
        }

        if (collision.transform.CompareTag("car"))
        {
            SetReward(-1.0f);
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
            SetReward(-2.0f);
        }
    }

    IEnumerator SwapMaterial(Material mat, float time)
    {
        floorRend.material = mat;
        yield return new WaitForSeconds(time);  // wait for 2 seconds
        floorRend.material = floorMat;
    }
}
