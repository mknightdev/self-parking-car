using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static int carsToSpawn;
    public GameObject escapeMenu;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    void Update()
    {
        PlayerInput();
    }

    public void SetCarsValue(int numOfCars)
    {
        carsToSpawn = numOfCars;
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene("TestingEnv_" + index);
    }
    
    public void ReturnToMenu()
    {   
        SceneManager.LoadScene(0);
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);

        escapeMenu = GameObject.Find("EscapeMenu");
    }

    private void PlayerInput()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0 && Input.GetKeyDown(KeyCode.Escape))
        {
            // Show/Hide escape menu
            escapeMenu.transform.GetChild(0).gameObject.SetActive(!escapeMenu.transform.GetChild(0).gameObject.activeInHierarchy);
        }
    }
}
