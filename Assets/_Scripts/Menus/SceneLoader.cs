using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static GameObject instance;
    public static int carsToSpawn;

    private CarAgent carAgent;

    private void Awake()
    {
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

    //private void OnEnable()
    //{
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    //void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    carAgent = FindObjectOfType<CarAgent>();
        
    //}

    //public void SetCarsValue(int numOfCars)
    //{
    //    carsToSpawn = numOfCars;
    //    Debug.Log($"Set new car value. Now: {carsToSpawn}");
    //}
}
