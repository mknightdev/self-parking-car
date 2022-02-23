using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ParkingAgent : Agent
{
    [Header("Agent")]
    private Rigidbody agentRb;
    private float horizontalInput;
    private float verticalInput;
    private float steerAngle;
    private float currentBreakForce;

    [Header("Car Settings")]
    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxAngle;

    [SerializeField] private WheelCollider frontLCollider;
    [SerializeField] private WheelCollider frontRCollider;
    [SerializeField] private WheelCollider backLCollider;
    [SerializeField] private WheelCollider backRCollider;

    [SerializeField] private Transform frontLTransform;
    [SerializeField] private Transform frontRTransform;
    [SerializeField] private Transform backLTransform;
    [SerializeField] private Transform backRTransform;

    [Header("Car Park")]
    public bool randomSlots;
    public int numOfCars;

    public Transform parkingSlots;
    private List<GameObject> parkingSlotsList;
    public List<GameObject> cars;
    private List<GameObject> carsList;

    // Called once 
    public override void Initialize()
    {
        // Get needed game objects
        agentRb = GetComponent<Rigidbody>();
        parkingSlotsList = new List<GameObject>();
        carsList = new List<GameObject>();

        // Populate car park list 
        //Debug.Log($"ParkingSlotsCount: {parkingSlots.childCount}");

    }

    private void FixedUpdate()
    {
        UpdateWheels();
    }

    // Called each beginning of an episode 
    public override void OnEpisodeBegin()
    {
        ResetAgent();
        ResetCarPark();
        SetupCarPark();
    }

    private void ResetAgent()
    {
        transform.localPosition = new Vector3(-21.75f, 1.25f, 9.75f);
        transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        agentRb.velocity = default(Vector3);
    }

    private void ResetCarPark()
    {

        for (int i = 0; i < parkingSlotsList.Count; i++)
        {
            parkingSlotsList[i].transform.GetChild(0).gameObject.SetActive(true);
        }

        // Clear lists
        parkingSlotsList.Clear();
        carsList.Clear();

        // Find all NPC cars, then delete them
        GameObject[] cars = GameObject.FindGameObjectsWithTag("car");
        foreach (GameObject car in cars)
        {
            Destroy(car);
        }

    }

    private void SetupCarPark()
    {
        //Transform m_parkingSlots = carPark.GetChild(0);

        // Iterate and add all parking slots
        for (int i = 0; i < parkingSlots.childCount; i++)
        {
            parkingSlotsList.Add(parkingSlots.GetChild(i).gameObject);
        }

        if (randomSlots)
        {
            numOfCars = Random.Range(parkingSlots.childCount / 2, parkingSlots.childCount - 1);
        }

        int m_randomValue = 0;
        List<int> m_randomValueList = new List<int>();

        // Loop until we have equal amount of values to cars
        while (m_randomValueList.Count != numOfCars)
        {
            // Generate random value
            m_randomValue = Random.Range(0, parkingSlotsList.Count);

            // If we don't have it, add it to the list
            if (!m_randomValueList.Contains(m_randomValue))
            {
                m_randomValueList.Add(m_randomValue);
            }
        }

        // Iterate and turn off target object based on how many cars already parked
        for (int i = 0; i < numOfCars; i++)
        {
            // Remove target object, as it's occupied
            parkingSlotsList[m_randomValueList[i]].transform.GetChild(0).gameObject.SetActive(false);

            // Spawn car in position
            carsList.Add(Instantiate(cars[Random.Range(0, cars.Count)], parkingSlotsList[m_randomValueList[i]].transform));
        }

        // Clear the list when all cars have spawned
        m_randomValueList.Clear();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];

        HandleMotor(controlSignal);
        HandleSteering(controlSignal);
        //UpdateWheels();
    }

    private void HandleMotor(Vector3 controlSignal)
    {
        frontLCollider.motorTorque = controlSignal.z * motorForce;
        frontRCollider.motorTorque = controlSignal.z * motorForce;
        //breakForce = isBreaking ? breakForce : 0f;
        //if (isBreaking)
        //{
        //    ApplyBreaking();
        //}
    }

    private void ApplyBreaking()
    {
        Debug.Log("Braking!");
        frontLCollider.brakeTorque = currentBreakForce;
        frontRCollider.brakeTorque = currentBreakForce;
        backLCollider.brakeTorque = currentBreakForce;
        backRCollider.brakeTorque = currentBreakForce;
    }

    private void HandleSteering(Vector3 controlSignal)
    {
        steerAngle = maxAngle * controlSignal.x;
        frontLCollider.steerAngle = steerAngle;
        frontRCollider.steerAngle = steerAngle;

    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLCollider, frontLTransform);
        UpdateSingleWheel(frontRCollider, frontRTransform);
        UpdateSingleWheel(backLCollider, backLTransform);
        UpdateSingleWheel(backRCollider, backRTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;

        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("goal"))
        {
            // Give reward

            // End episode
            EndEpisode();
        }
    }

}
