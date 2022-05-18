using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static int carsToSpawn;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SetCarsValue(int numOfCars)
    {
        carsToSpawn = numOfCars;
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene("TestingEnv_" + index);
    }
}
