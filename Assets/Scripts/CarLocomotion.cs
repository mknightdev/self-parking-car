using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLocomotion : MonoBehaviour
{
    [Header("Car Park")]
    public Transform parkingSlots;
    public Transform goalParkingSlot;

    private float horizontalInput;
    private float verticalInput;
    private float steerAngle;
    private float currentBreakForce;
    private bool isBreaking;

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

    private void Start()
    {
        for (int i = 0; i < parkingSlots.childCount; i++)
        {
            // Check if gameobject is active
            if (parkingSlots.GetChild(i).GetChild(0).gameObject.activeSelf)
            {
                // Calculate the distance for each parking slot
                var newDistance = Vector3.Distance(this.transform.position, parkingSlots.GetChild(i).GetChild(0).position);
                var distance = 0.0f;
                if (distance == 0.0f || newDistance < distance)
                {
                    // Set the distance to the closest distance
                    distance = newDistance;
                    goalParkingSlot = parkingSlots.GetChild(i).GetChild(0);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        frontLCollider.motorTorque = verticalInput * motorForce;
        frontRCollider.motorTorque = verticalInput * motorForce;
        breakForce = isBreaking ? breakForce : 0f;
        if (isBreaking)
        {
            ApplyBreaking();
        }
    }

    private void ApplyBreaking()
    {
        Debug.Log("Braking!");
        frontLCollider.brakeTorque = currentBreakForce;
        frontRCollider.brakeTorque = currentBreakForce;
        backLCollider.brakeTorque = currentBreakForce;
        backRCollider.brakeTorque = currentBreakForce;
    }

    private void HandleSteering()
    {
        steerAngle = maxAngle * horizontalInput;
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

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("goal"))
        {
            // Give reward
            Debug.Log("goal reached");
        }
    }
}
