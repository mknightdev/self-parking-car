using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class NPCManager : MonoBehaviour
{
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Rigidbody npcRb;

    private void Start()
    {
        npcRb = this.GetComponent<Rigidbody>();

        // Set default values
        defaultPosition = this.transform.localPosition;
        defaultRotation = this.transform.localRotation;
    }
    
    public void ResetNPC()
    {
        this.transform.localPosition = defaultPosition;
        this.transform.localRotation = defaultRotation;

        this.npcRb.velocity = Vector3.zero;
        this.npcRb.angularVelocity = Vector3.zero;
    }
}
