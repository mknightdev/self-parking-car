using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneRestarter))]
public class SceneRestarterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneRestarter myScript = (SceneRestarter)target;  
        if (GUILayout.Button("Reset Counter"))
        {
            myScript.ResetCounter();
        }
    }
}
