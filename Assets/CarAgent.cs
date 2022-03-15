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
    private bool rewardGave = false;

    private float directionDot;
    

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
        rewardGave = false;
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
        this.transform.localPosition = new Vector3(0.0f, 0.5f, -3.0f);
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

                this.targets[i].gameObject.layer = 0;

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

                this.target.gameObject.layer = 6;
            }

            // Reset Cars
            //targets[i].GetChild(0).Find("Car").GetComponent<NPCManager>().ResetNPC();
        }

    }

    private void Update()
    {
        //Debug.Log($"Direction Dot: {directionDot}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /// Observations: 12

        // Target and Agent Pos, Rot
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(target.localRotation);
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

        // Direction of the goal
        Vector3 goalForward = target.transform.forward;
        directionDot = Vector3.Dot(this.transform.forward, goalForward);    // -1 to 1; Opposite rotation; Correct rotation
        sensor.AddObservation(directionDot);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Last distance
        float distance = Vector3.Distance(this.transform.localPosition, this.target.localPosition);

        if (!rewardGave && distance < 2.5f)
        {
            AddReward(5.0f);
            rewardGave = true;
        }

        // Reward if relatively the same orienation
        if (directionDot > 0.95)
        {
            AddReward(0.1f);
        }

        //// Punish if it's not the same rotation
        //if (directionDot < 0)
        //{
        //    AddReward(-0.05f);
        //}

        // Get action index for movement 
        int movement = actions.DiscreteActions[0];
        
        // Get action index for steering
        int steering = actions.DiscreteActions[1];

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

        AddReward(-1.0f / MaxStep); // Encourage the agent to reach the goal faster

        // Stats
        GlobalStats.UpdateText();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
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

            AddReward(10.0f);
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
        yield return new WaitForSeconds(0.0f);
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
            AddReward(-2.5f);
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