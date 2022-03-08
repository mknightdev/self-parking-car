using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class NPCManager : MonoBehaviour
{
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    private void Start()
    {
        // Set default values
        defaultPosition = this.transform.localPosition;
        defaultRotation = this.transform.localRotation;
    }
    
    public void ResetNPC()
    {
        // Reset
        this.transform.localPosition = defaultPosition;
        this.transform.localRotation = defaultRotation;
    }
}
