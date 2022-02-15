using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public bool isPowered;  // Wheel will move when acceleration is applied
    public float maxAngle;  // Maximum steer angle
    public float offset;    // 

    private float turnAngle;
    [SerializeField] private WheelCollider wheelCollider;
    private Transform wheelTransform;

    // Start is called before the first frame update
    void Start()
    {
        //wheelCollider = GetComponentInChildren<WheelCollider>();
        //wheelTransform = transform.Find("wheel_mesh");
    }

    public void Steer(float steerInput)
    {
        turnAngle = steerInput * maxAngle + offset;
        wheelCollider.steerAngle = turnAngle;
    }

    public void Accelerate(float powerInput)
    {
        if (isPowered)
        {
            wheelCollider.motorTorque = powerInput;
        }
        else
        {
            wheelCollider.brakeTorque = 0;
        }
    }

    public void UpdatePos()
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.transform.position = pos;
        wheelTransform.transform.rotation = rot;
    }
}
