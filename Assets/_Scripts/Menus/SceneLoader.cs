using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static GameObject instance;
    public static int carsToSpawn;  // Set within the menu. 


    private void Awake()
    {
        // Persists between scenes to keep track,
        // and get access to carsToSpawn.
        DontDestroyOnLoad(this);

        if (instance == null)
        {
            instance = gameObject;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
