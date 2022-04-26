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

    private Vector3 dirToTarget;
    private bool agentParked = false;
    private bool firstRun = true;
    private bool spaceCPGave = false;

    private Quaternion defaultRotation;

    private CarParkManager carParkManager;
    private bool isCollidingWithCar;
    private float timerCountdown = 1.5f;

    private void Start()
    {
        // Get the agent's rigidbody
        agentRb = this.GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        GlobalStats.UpdateText();

        carParkManager = transform.parent.GetComponent<CarParkManager>();

        agentParked = true;

        defaultRotation = transform.localRotation;

        //this.cars = new List<GameObject>();

        // Get the environment settings
        envSettings = FindObjectOfType<EnvSettings>();

        // Get the floor
        floorRend = this.transform.parent.Find("Environment").Find("Floor").GetComponent<MeshRenderer>();
        floorMat = floorRend.material;

        carParkManager.GetAllTargets();
    }

    public override void OnEpisodeBegin()
    {
        spaceCPGave = false;
        rewardGave = false;
        isCollidingWithCar = false;
        timerCountdown = 1.5f;
        GlobalStats.episode += 1;

        if (!agentParked)
        {
            GlobalStats.fail += 1;  // If the agent didn't park, we add a fail
        }
        agentParked = false;

        carParkManager.CleanCarPark();

        // Reset Acceleration
        this.carLocomotion.currentAcceleration = 0.0f;

        // Move agent back to starting position
        this.transform.localPosition = new Vector3(7.5f, 0.5f, -9.0f);
        this.transform.localRotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);

        // Zero the velocity
        this.agentRb.velocity = Vector3.zero;
        this.agentRb.angularVelocity = Vector3.zero;


        // Get the selected target
        target = carParkManager.SetMainTarget();

        carParkManager.SetupCarPark();
    }

    private void Update()
    {
        GlobalStats.completedEpisodes = CompletedEpisodes;
        //Debug.Log($"dirToTarget: {dirToTarget.x}, {dirToTarget.y}, {dirToTarget.z}");

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
        // Target and Agent Pos, Rot
        sensor.AddObservation(
            this.transform.InverseTransformPoint(target.localPosition));

        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.transform.localRotation);

        //// Observe speed, turn angle, brake
        sensor.AddObservation(this.carLocomotion.currentAcceleration);
        //sensor.AddObservation(this.carLocomotion.currentBrakeForce);
        sensor.AddObservation(this.carLocomotion.currentTurnAngle);

        // Direction of Goal
        dirToTarget = (this.target.position - this.transform.position).normalized;

        sensor.AddObservation(
            this.transform.InverseTransformDirection(dirToTarget));

        sensor.AddObservation(this.target.forward);
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
        float distance = Vector3.Distance(this.transform.localPosition, this.target.localPosition);

        if (!rewardGave && distance < 2.5f)
        {
            AddReward(2.5f);
            rewardGave = true;
        }

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



    private float CheckOrientation()
    {
        float directionDot = Vector3.Dot(this.transform.forward, target.transform.forward);

        float orientationBonus = 0.0f;

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

    private float CheckRotation()
    {
        float angleBonus = 0.0f;
        if (Mathf.Abs(Vector3.Dot(this.transform.up, Vector3.down)) < 0.125f)
        {
            // Car is neither up or down, with 1/8 of a 90 degree rotation
            angleBonus = -90 / 1000.0f;
        }
        else
        {
            angleBonus =  90 / 1000.0f;
        }

        return angleBonus;
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
            AddReward(-0.3f);
        }
        else if (collision.transform.CompareTag("car"))
        {
            AddReward(-0.075f);
            isCollidingWithCar = true;
        }
        else if (collision.transform.CompareTag("bumper"))  // These are at the back of each parking spot
        {
            AddReward(-0.05f);
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("wall"))
        {
            AddReward(-0.005f);
        }
        //else if (collision.transform.CompareTag("car"))
        //{
        //    AddReward(-0.00020f);
        //}
        else if (collision.transform.CompareTag("car") && isCollidingWithCar)
        {
            if (timerCountdown <= 0)
            {
                SetReward(-0.1f);
                EndEpisode();
                StartCoroutine(SwapMaterial(envSettings.failMat, 2.0f));
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isCollidingWithCar = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("yellowLine"))
        {
            AddReward(-0.05f);
        }
        else if (other.transform.CompareTag("path"))
        {
            GlobalStats.fail += 1;
            AddReward(-0.1f);
            EndEpisode();
            StartCoroutine(SwapMaterial(envSettings.failMat, 2.0f));
        }
        else if (other.transform.CompareTag("spaceCheckPoint") && !spaceCPGave)
        {
            spaceCPGave = true;
            AddReward(0.1f);
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

    IEnumerator SwapMaterial(Material mat, float time)
    {
        floorRend.material = mat;   // Swap to win or fail material
        yield return new WaitForSeconds(time);  // Wait for 2 seconds
        floorRend.material = floorMat;  // Swap back to default material
    }
}