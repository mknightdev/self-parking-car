using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    [Header("Target Settings")]
    public Transform chosenTarget;  // The chosen target for the given episode.  
    public List<Transform> targets; // List of potential targets within the car park. 
    public float parkedWaitTime = 1.0f; // Wait time for how long the agent has stayed in the space before succeeding.

    [Header("Environment Settings")]
    public EnvSettings envSettings; // Access to floor materials.

    private MeshRenderer floorRend; // Floor renderer to change the material. 
    private Material floorMat;  // Default floor material. 
    private CarParkManager carParkManager;  // Used to setup and reset the car park. 

    [Header("Agent Settings")]
    public CarLocomotion carLocomotion; // Access to apply forces to the agent.
    public float lerpSpeed = 50f;   // Smoothes out the car locomotion forces.

    private Rigidbody agentRb;  // Used to zero out velocities when OnEpisodeBegin() is called. 
    private Quaternion defaultRotation; // Used to reset the agent when OnEpisodeBegin() is called.
    private Vector3 defaultPosition;

    // Rewards
    private bool rewardGave = false;    // If the distance reward was given. 
    private bool spaceCPGave = false;   // If the checkpoint reward was given. 

    // Detection
    private bool agentParked = false;       // If the agent parked successfully. 
    private bool hasStopped = false;        // If the agent has stopped or not, after checking.
    private bool hasStoppedCheck = false;   // The first check to see if the agent has stopped.
    private bool isCollidingWithCar;        // Starts the timer when we start colliding with a car. 
    private float timerCountdown = 1.5f;    // How long the agent collided with a car for.
    private Vector3 dirToTarget;

    // Input
    private float verticalInput;    // Player input when heuristic is set.
    private float horizontalInput;

    private void Start()
    {
        // Get the agent's rigidbody
        agentRb = this.GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        // Update UI text. Shows episodes, success, fails and success rate. 
        GlobalStats.UpdateText();
        
        // Find the car park manager. 
        carParkManager = transform.parent.GetComponent<CarParkManager>();

        // Set to true to prevent a false fail (UI Only).
        agentParked = true;

        // Set the default rotation and position of the agent.
        defaultPosition = this.transform.localPosition;
        defaultRotation = this.transform.localRotation;

        // Get the environment settings
        envSettings = FindObjectOfType<EnvSettings>();

        // Get the floor renderer.
        floorRend = this.transform.parent.Find("Environment").Find("Floor").GetComponent<MeshRenderer>();
        floorMat = floorRend.material;  // Set default material. 

        // Find all the targets within the car park.
        carParkManager.GetAllTargets();
    }

    public override void OnEpisodeBegin()
    {
        // Reset detection bools. 
        spaceCPGave = false;
        rewardGave = false;
        isCollidingWithCar = false;
        timerCountdown = 1.5f;

        // Increase our episode count. 
        GlobalStats.episode += 1;

        // If we failed to park last episode, increase the fail count. 
        if (!agentParked)
        {
            GlobalStats.fail += 1;  // If the agent didn't park, we add a fail
        }
        agentParked = false;    // Reset the bool for this episode. 

        carParkManager.ClearCarPark();  // Clears the car park by removing all cars.

        // Reset Acceleration
        this.carLocomotion.currentAcceleration = 0.0f;

        // Move agent back to starting position
        this.transform.localPosition = defaultPosition;
        this.transform.localRotation = defaultRotation;

        // Zero the velocity
        this.agentRb.velocity = Vector3.zero;
        this.agentRb.angularVelocity = Vector3.zero;


        // Get the selected target
        chosenTarget = carParkManager.SetMainTarget();

        // Sets up the car park with cars and hides relevant target meshes. 
        carParkManager.SetupCarPark();
    }

    private void Update()
    {
        // Update the UI for our completed episodes. 
        GlobalStats.completedEpisodes = CompletedEpisodes;

        // Start the timer countdown if we are colliding with a car. 
        if (isCollidingWithCar)
        {
            timerCountdown -= Time.deltaTime;
            if (timerCountdown < 0)
            {
                timerCountdown = 0;
            }
        }
        else
        {
            timerCountdown = 1.5f;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /// Observations: 17
        // Target local position
        sensor.AddObservation(this.transform.InverseTransformPoint(chosenTarget.localPosition));

        // Agent's local position and rotation
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.transform.localRotation);

        // Acceleration and turn angle of the agent
        sensor.AddObservation(this.carLocomotion.currentAcceleration);
        sensor.AddObservation(this.carLocomotion.currentTurnAngle);

        // Calculate the direction to the target
        dirToTarget = (this.chosenTarget.position - this.transform.position).normalized;

        // Direction to goal in local position
        sensor.AddObservation(this.transform.InverseTransformDirection(dirToTarget));

        // Forward direction of the target, useful for orientation reward
        sensor.AddObservation(this.chosenTarget.forward);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Agent fell off
        if (this.transform.localPosition.y < -2.0f)
        {
            SetReward(-10.0f);
            EndEpisode();
            StartCoroutine(SwapMaterial(envSettings.failMat, 2.0f));
        }

        // Last distance
        float distance = Vector3.Distance(this.transform.localPosition, this.chosenTarget.localPosition);

        // Give reward for getting closer to the target. 
        if (!rewardGave && distance < 2.5f)
        {
            AddReward(2.5f);
            rewardGave = true;
        }

        // Get action index for movement 
        int movement = actions.DiscreteActions[0];
        
        // Get action index for steering
        int steering = actions.DiscreteActions[1];

        // Movement actions. 
        switch (movement)
        {
            // Forward.
            case 0: // Negative
                carLocomotion.Accelerate(Mathf.Lerp(0, 1, lerpSpeed * Time.deltaTime));
                break;

            // Backwards.
            case 1:
                carLocomotion.Accelerate(-Mathf.Lerp(0, 1, lerpSpeed * Time.deltaTime));
                //Debug.Log($"Backward");
                break;

            // Don't move. 
            case 2:
                carLocomotion.Accelerate(0);
                break;
        }

        switch (steering)
        {
            // Turn left. 
            case 0: // Negative
                carLocomotion.Steer(-Mathf.Lerp(0, 1, lerpSpeed * Time.deltaTime));
                break;

            // Turn right.
            case 1:
                carLocomotion.Steer(Mathf.Lerp(0, 1, lerpSpeed * Time.deltaTime));
                break;

            // Don't turn. 
            case 2:
                carLocomotion.Steer(0);
                break;
        }

        AddReward(-1.0f / MaxStep); // Encourage the agent to reach the goal faster

        // Update the UI stats.
        GlobalStats.UpdateText();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Player input. 
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


    /// <summary>
    /// Calculate the orientation of the agent compared to the direction of the chosen target.
    /// </summary>
    /// <returns></returns>
    private float CheckOrientation()
    {
        float directionDot = Vector3.Dot(this.transform.forward, chosenTarget.transform.forward);

        float orientationBonus = 0.0f;

        // Give a positive or negative reward based on the orientation.
        if (directionDot > 0)
        {
            orientationBonus = directionDot / 50.0f;
        }
        else if (directionDot < 0)
        {
            orientationBonus = -directionDot / 50.0f;
        }

        return orientationBonus;
    }

    /// <summary>
    /// Calculate the angle bonus to see if the agent is on its side. 
    /// 
    /// </summary>
    /// <returns></returns>
    private float CheckRotation()
    {
        float angleBonus = 0.0f;

        // Agent receives a positive or negative reward if it is up-right or on its side. 
        /// The calculation of the angle was originally written by Eno-Khaon.
        if (Mathf.Abs(Vector3.Dot(this.transform.up, Vector3.down)) < 0.125f) /// end of calculation. 
        {

            // Car is neither up or down, with 1/8 of a 90 degree rotation.
            angleBonus = -90 / 1000.0f;
        }
        else
        {
            angleBonus =  90 / 1000.0f;
        }

        return angleBonus;
    }

    /// <summary>
    /// Timer to check if the agent has stopped before parking in the chosen target.
    /// </summary>
    IEnumerator HasParked()
    {
        yield return new WaitForSeconds(parkedWaitTime);
        hasStopped = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Punishments for entering a collision with obstacles.

        if (collision.transform.CompareTag("wall"))
        {
            AddReward(-0.3f);
        }
        else if (collision.transform.CompareTag("car"))
        {
            AddReward(-0.075f);
            isCollidingWithCar = true;  // Start reducing the colliding with car timer. 
        }
        else if (collision.transform.CompareTag("bumper"))  // These are at the back of each parking spot
        {
            AddReward(-0.05f);
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        // Small increment punishments for staying in collision with obstacles. 

        if (collision.transform.CompareTag("wall"))
        {
            AddReward(-0.005f);
        }
        else if (collision.transform.CompareTag("car") && isCollidingWithCar)
        {
            if (timerCountdown <= 0)
            {
                // If the agent is colliding with a car and the timer is 0, punish it and end the episode.
                SetReward(-0.1f);
                EndEpisode();
                StartCoroutine(SwapMaterial(envSettings.failMat, 2.0f));
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isCollidingWithCar = false; // No longer colliding with a car. 
    }

    private void OnTriggerEnter(Collider other)
    {
        // Punishments and rewards for entering a trigger with obstacles.

        if (other.transform.CompareTag("yellowLine"))
        {
            AddReward(-0.05f);
        }
        else if (other.transform.CompareTag("path"))
        {
            // If the agent hits a path, we punish and end the episode.
            GlobalStats.fail += 1;

            AddReward(-0.1f);
            EndEpisode();
            StartCoroutine(SwapMaterial(envSettings.failMat, 2.0f));
        }
        else if (other.transform.CompareTag("spaceCheckPoint") && !spaceCPGave)
        {
            // If the agent goes through a checkpoint, reward them if it has not be already given.
            spaceCPGave = true;
            AddReward(0.1f);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        // Check if we have stopped within the parking space for the given time. 
        if (other.CompareTag("target") && !hasStoppedCheck)
        {
            hasStoppedCheck = true;
            StartCoroutine(HasParked());
        }

        // If we have stopped for the duration required, check our orientation and angle 
        // in relation to the parking space and apply it as a bonus to the reward. 
        if (other.CompareTag("target") && hasStopped)
        {
            // Increase success count.
            GlobalStats.success += 1;

            agentParked = true;

            // Check orientation
            float orientationBonus = 0.0f;
            orientationBonus = CheckOrientation();

            // Check rotation
            float angleBonus = 0.0f;
            angleBonus = CheckRotation();

            AddReward(5.0f + orientationBonus + angleBonus);
            EndEpisode();
            hasStopped = false;
            hasStoppedCheck = false;
            StartCoroutine(SwapMaterial(envSettings.winMat, 2.0f));
        }

        if (other.transform.CompareTag("yellowLine"))
        {
            AddReward(-0.0002f);
        }
    }

    /// <summary>
    /// Swaps the floor material to visually indicate a success or fail, in addition to the stats UI.
    /// </summary>
    /// <param name="mat">The material to change the floor with.</param>
    /// <param name="time">How long the material changes for.</param>
    /// <returns></returns>
    IEnumerator SwapMaterial(Material mat, float time)
    {
        floorRend.material = mat;   // Swap to win or fail material
        yield return new WaitForSeconds(time);  // Wait for X seconds
        floorRend.material = floorMat;  // Swap back to default material
    }

    /// <summary>
    /// Function called from the 'Menu' button within the escape menu.
    /// Shuts down the academy, meaning it can intialise again in new scenes.
    /// </summary>
    public void DisposeAcademy()
    {
        Academy.Instance.Dispose();
    }
}