using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private CarAgent carAgent;  // Used to grant access to the agent-inherited script. 

    private void Awake()
    {
        // If we aren't in the main menu, retrieve the agent.
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            carAgent = FindObjectOfType<CarAgent>();
        }
    }

    /// <summary>
    /// Before we return to the main menu from the escape menu,
    /// dispose of the Academy, so it can re-intialise in the next scene.
    /// </summary>
    public void ReturnToMenu()
    {
        // Before we return, dispose of the Academy.
        carAgent.DisposeAcademy();
        GlobalStats.ResetStats();
        
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        PlayerInput();
    }

    /// <summary>
    /// Detects player input. 
    /// Contains:   
    ///     Shows and hides the escape menu.
    /// </summary>
    private void PlayerInput()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0 && Input.GetKeyDown(KeyCode.Escape))
        {
            // Show/Hide escape menu
            transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeInHierarchy);
        }
    }

    /// <summary>
    /// Loads the selected testing environment based on its index.
    /// </summary>
    /// <param name="index">The testing environment number is the same as its build index.</param>
    public void LoadScene(int index)
    {
        SceneManager.LoadScene("TestingEnv_" + index);
    }

    /// <summary>
    /// Sets the number of cars to spawn based on the scenario selected.
    /// </summary>
    /// <param name="numOfCars">Number of cars to spawn in the environment.</param>
    public void StoreCarValue(int numOfCars)
    {
        SceneLoader.carsToSpawn = numOfCars;
    }

    /// <summary>
    /// Quit the application.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
