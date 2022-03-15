using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    public GameObject carObj;
    public CarLocomotion carLocomotion;
    public float lerpSpeed = 50f;

    public EnvSettings envSettings;
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

    private float verticalInput;
    private float horizontalInput;

    private List<GameObject> cars;
    

    private void Start()
    {
        // Get the agent's rigidbody
        agentRb = this.GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        GlobalStats.UpdateText();

        this.cars = new List<GameObject>();

        // Get target
        //target = this.transform.parent.Find("Target").transform;
        //Debug.Log($"TargetLocal: {target.position}");

        // Get the environment settings
        envSettings = FindObjectOfType<EnvSettings>();

        // Get the floor
        floorRend = this.transform.parent.Find("Environment").Find("Floor").GetComponent<MeshRenderer>();
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

        if (this.cars.Count > 0)
        {
            // Destroy all cars
            for (int i = 0; i < this.cars.Count; i++)
            {
                Destroy(this.cars[i].gameObject);
            }
        }

        // Clear the car list
        this.cars.Clear();

        // Reset Acceleration
        this.carLocomotion.currentAcceleration = 0.0f;

        // Move agent back to starting position
        this.transform.localPosition = new Vector3(0.0f, 0.5f, -6.5f);
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

                // Spawn Car
                GameObject temp = Instantiate(carObj, targets[i].GetChild(0));
                temp.name = temp.name.Replace("(Clone)","");    // Removes clone in the name

                var wheelCols = temp.transform.Find("Wheel Colliders");
                // Zero out
                for (int j = 0; j < wheelCols.childCount; j++)
                {
                    // Stops car from moving when spawned
                    wheelCols.GetChild(j).GetComponent<WheelCollider>().motorTorque = 0.0f;
                    wheelCols.GetChild(j).GetComponent<WheelCollider>().brakeTorque = 1000.0f;
                }
        
                this.cars.Add(temp);
            }
            else
            {
                // Show the target
                target.GetComponent<MeshRenderer>().enabled = true;
                target.GetComponent<BoxCollider>().enabled = true;
            }

            // Reset Cars
            //targets[i].GetChild(0).Find("Car").GetComponent<NPCManager>().ResetNPC();
        }

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /// Observations: 12

        // Target and Agent Pos, Rot
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.transform.localRotation);

        //sensor.AddObservation(this.agentRb.velocity.x);
        //sensor.AddObservation(this.agentRb.velocity.z);

        // Speed
        //sensor.AddObservation(this.moveSpeed);

        // Observe speed, turn angle, brake
        sensor.AddObservation(this.carLocomotion.currentAcceleration);
        sensor.AddObservation(this.carLocomotion.currentBrakeForce);
        sensor.AddObservation(this.carLocomotion.currentTurnAngle);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get action index for movement 
        int movement = actions.DiscreteActions[0];
        
        // Get action index for steering
        int steering = actions.DiscreteActions[1];

        //verticalInput = actions.DiscreteActions[0];
        //horizontalInput = actions.DiscreteActions[1];

        switch (movement)
        {
            case 0: // Negative
                carLocomotion.Accelerate(Mathf.Lerp(0, 1, lerpSpeed * Time.deltaTime));
                //Debug.Log($"Forward");
                break;
            case 1:
                carLocomotion.Accelerate(-Mathf.Lerp(0, 1, lerpSpeed * Time.deltaTime));
                //Debug.Log($"Backward");
                break;
            case 2:
                carLocomotion.Accelerate(0);
                //Debug.Log("DontMove");
                break;
        }

        switch (steering)
        {
            case 0: // Negative
                carLocomotion.Steer(-Mathf.Lerp(0, 1, lerpSpeed * Time.deltaTime));
                //Debug.Log($"TurnLeft");
                break;
            case 1:
                carLocomotion.Steer(Mathf.Lerp(0, 1, lerpSpeed * Time.deltaTime));
                //Debug.Log($"TurnRight");
                break;
            case 2:
                carLocomotion.Steer(0);
                //Debug.Log("DontTurn");
                break;
        }

        // Rewards
        float distance = Vector3.Distance(this.transform.localPosition, target.localPosition);
        //Debug.Log($"Distance: {distance}");

        if (distance < 6.0f)
        {
            SetReward(1.0f);
        }

        if (distance < 1.5f)
        {
            // If the agent has got closer, reward it
            SetReward(1.5f);
        }
        else
        {
            // If the agent hasn't got closer, punish it
            SetReward(-0.01f);
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
        //var continousActionsOut = actionsOut.ContinuousActions;
        //continousActionsOut[0] = Input.GetAxis("Horizontal");
        //continousActionsOut[1] = Input.GetAxis("Vertical");

        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        var discreteActionsOut = actionsOut.DiscreteActions;

        // Accelerating/Reversing
        if (verticalInput < 0)
        {
            discreteActionsOut[0] = 1;  // Foward
        }
        else if (verticalInput > 0)
        {
            discreteActionsOut[0] = 0;  // Backward
        }
        else
        {
            discreteActionsOut[0] = 2;  // Nothing
        }

        // Steering
        if (horizontalInput < 0)
        {
            discreteActionsOut[1] = 0;  // Turn Left
        }
        else if (horizontalInput > 0)
        {
            discreteActionsOut[1] = 1;  // Turn Right
        }
        else
        {
            discreteActionsOut[1] = 2;  // Nothing
        }

        //Debug.Log($"Steering: {discreteActionsOut[1]}");
        //Debug.Log($"Movement: {discreteActionsOut[0]}");

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("yellowLine"))
        {
            AddReward(-3.0f);
        }
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

        if (other.CompareTag("yellowLine"))
        {
            AddReward(-0.05f);
        }
    }

    IEnumerator HasParked()
    {
        yield return new WaitForSeconds(1.5f);
        hasStopped = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("wall"))
        {
            AddReward(-7.0f);
        }

        if (collision.transform.CompareTag("car"))
        {
            Debug.Log("Collision Enter");
            AddReward(-0.5f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("wall"))
        {
            AddReward(-0.01f);
        }

        if (collision.transform.CompareTag("car"))
        {
            Debug.Log("Collision Stay");
            AddReward(-0.05f);
        }
    }

    IEnumerator SwapMaterial(Material mat, float time)
    {
        floorRend.material = mat;
        yield return new WaitForSeconds(time);  // wait for 2 seconds
        floorRend.material = floorMat;
    }
}