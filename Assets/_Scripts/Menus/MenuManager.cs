using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private CarAgent carAgent;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            carAgent = FindObjectOfType<CarAgent>();
        }
    }

    public void ReturnToMenu()
    {
        // Before we return, dispose of the Academy.
        carAgent.DisposeAcademy();
        
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        PlayerInput();
    }

    private void PlayerInput()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0 && Input.GetKeyDown(KeyCode.Escape))
        {
            // Show/Hide escape menu
            transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeInHierarchy);
        }
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene("TestingEnv_" + index);
    }

    public void StoreCarValue(int numOfCars)
    {
        SceneLoader.carsToSpawn = numOfCars;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
