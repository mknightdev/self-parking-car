using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// The following script is based, with  modifications from :-
/// b3agz (2021). Basic Car Movement in Unity [online].
/// [Accessed 9 Mar 2022]. Available from: <https://www.youtube.com/watch?v=QQs9MWLU_tU>

public class CarLocomotion : MonoBehaviour
{
    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontRight;
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider backRight;
    [SerializeField] private WheelCollider backLeft;
    [SerializeField] private List<WheelCollider> wheelCols; // List of wheel colliders, useful for X-wheel drive. 

    [Header("Wheel Meshes")]
    [SerializeField] private Transform frontRightTransform;
    [SerializeField] private Transform frontLeftTransform;
    [SerializeField] private Transform backRightTransform;
    [SerializeField] private Transform backLeftTransform;

    [Header("Vehicle Force Properties")]
    [SerializeField] private bool isFourWheelDrive = false;
    public float acceleration = 500f;   // How fast the vehicle can move.
    public float brakeForce = 600f;     // How quickly the car comes to a stop.
    public float maxTurnAngle = 30f;    // Maximum turn the vehicle can perform.

    // Current force values.
    [HideInInspector] public float currentAcceleration = 0.0f;
    [HideInInspector] public float currentBrakeForce = 0.0f;
    [HideInInspector] public float currentTurnAngle = 0.0f;

    private void FixedUpdate()
    {
        PlayerInput();

        // Update vehicle forces.
        UpdateAcceleration();
        UpdateBrakeForce();
        UpdateTurnAngle();

        // Update all the wheels with the changed values.
        UpdateWheel(frontRight, frontRightTransform);
        UpdateWheel(frontLeft, frontLeftTransform);
        UpdateWheel(backRight, backRightTransform);
        UpdateWheel(backLeft, backLeftTransform);
    }

    private void PlayerInput()
    {
        // Apply brake on space.
        if (Input.GetKey(KeyCode.Space))
        {
            currentBrakeForce = brakeForce;
        }
        else
        {
            // Reset brake force if space is not being pressed.
            currentBrakeForce = 0.0f;
        }
    }

    /// <summary>
    /// Updates and applies the turn angle of the two front wheels,
    /// with the current turn angle.
    /// </summary>
    private void UpdateTurnAngle()
    {
        // Apply the turn angle on input.
        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;
    }

    /// <summary>
    /// Updates and applie the acceleration to the wheels. 
    /// 2-wheel drive updates the front two wheels.
    /// 4-wheel drive loops through the all the wheels.
    /// </summary>
    private void UpdateAcceleration()
    {
        if (isFourWheelDrive)
        {
            for (int i = 0; i < wheelCols.Count; i++)
            {
                wheelCols[i].motorTorque = currentAcceleration;
            }
        }
        else
        {
            // Assume we are 2-wheel drive
            // Apply acceleration to the front wheels.
            frontRight.motorTorque = currentAcceleration;
            frontLeft.motorTorque = currentAcceleration;
        }
    }

    /// <summary>
    /// Updates and applies the brake force to all wheels.
    /// </summary>
    private void UpdateBrakeForce()
    {
        // Apply brake to all wheels.
        for (int i = 0; i < wheelCols.Count; i++)
        {
            wheelCols[i].brakeTorque = currentBrakeForce;
        }
    }

    /// <summary>
    /// Update the current acceleration based on the input passed-through.
    /// </summary>
    /// <param name="value">Input value from the player, or the agent.</param>
    public void Accelerate(float value)
    {
        currentBrakeForce = 0.0f;
        currentAcceleration = acceleration * value;
    }

    /// <summary>
    /// Update the current turn angle based on the input passed-through.
    /// </summary>
    /// <param name="value">Input value from the player, or the agent.</param>
    public void Steer(float value)
    {
        currentBrakeForce = 0.0f;
        currentTurnAngle = maxTurnAngle * value;
    }

    /// <summary>
    /// Update the wheel positions and rotations to show the forces applied. 
    /// </summary>
    /// <param name="wheelCol">Used to get the position in the world.</param>
    /// <param name="wheelTransform">Used to update the rotation and position of the wheels.</param>
    void UpdateWheel(WheelCollider wheelCol, Transform wheelTransform)
    {
        Vector3 wheelPos;
        Quaternion wheelRot;

        // Gets the position and rotation of wheel collider.
        wheelCol.GetWorldPose(out wheelPos, out wheelRot);

        // Update the wheel position and rotation based on collider
        wheelTransform.position = wheelPos;
        wheelTransform.rotation = wheelRot;
    }
}
