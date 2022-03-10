using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLocomotion : MonoBehaviour
{
    // Wheels
    [SerializeField] private WheelCollider frontRight;
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider backRight;
    [SerializeField] private WheelCollider backLeft;

    // Reference to wheel mesh
    [SerializeField] private Transform frontRightTransform;
    [SerializeField] private Transform frontLeftTransform;
    [SerializeField] private Transform backRightTransform;
    [SerializeField] private Transform backLeftTransform;

    public float acceleration = 500f;
    public float brakeForce = 600f; // How quickly the car comes to a stop
    public float maxTurnAngle = 30f;

    [HideInInspector] public float currentAcceleration = 0.0f;
    [HideInInspector] public float currentBrakeForce = 0.0f;
    [HideInInspector] public float currentTurnAngle = 0.0f;

    private void FixedUpdate()
    {
        // W and S for forward and reverse
        //currentAcceleration = acceleration * Input.GetAxis("Vertical");

        // Apply brake on space
        if (Input.GetKey(KeyCode.Space))
        {
            currentBrakeForce = brakeForce;
        }
        else
        {
            currentBrakeForce = 0.0f;
        }

        // Apply acceleration to the front wheels
        frontRight.motorTorque = currentAcceleration;
        frontLeft.motorTorque = currentAcceleration;

        frontRight.brakeTorque = currentBrakeForce;
        frontLeft.brakeTorque = currentBrakeForce;
        backRight.brakeTorque = currentBrakeForce;
        backLeft.brakeTorque = currentBrakeForce;

        // Steering
        //currentTurnAngle = maxTurnAngle * Input.GetAxis("Horizontal");
        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;

        UpdateWheel(frontRight, frontRightTransform);
        UpdateWheel(frontLeft, frontLeftTransform);
        UpdateWheel(backRight, backRightTransform);
        UpdateWheel(backLeft, backLeftTransform);
    }

    public void Accelerate(float value)
    {
        currentAcceleration = acceleration * value;
    }

    public void Steer(float value)
    {
        currentTurnAngle = maxTurnAngle * value;
    }

    void UpdateWheel(WheelCollider col, Transform trans)
    {
        // Get wheel collider state
        Vector3 position;
        Quaternion rotation;

        // Gets the position and rotation of wheel collider
        col.GetWorldPose(out position, out rotation);

        // Set the wheel position and rotation based on collider
        trans.position = position;
        trans.rotation = rotation;

    }
}
