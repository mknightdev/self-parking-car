using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneRestarter : MonoBehaviour
{
    public int counter = 0;
    public TextMeshProUGUI counterText;

    public TextMeshProUGUI numOfCarsText;
    public TextMeshProUGUI freeSlotsText;

    private void Start()
    {
        counter = PlayerPrefs.GetInt("counter", 1);
        counterText.text = $"#{counter}";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { RestartScene(); }
    }

    public void RestartScene()
    {
        counter++;
        PlayerPrefs.SetInt("counter", counter);
        PlayerPrefs.Save();

        // Restart the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetCounter()
    {
        counter = 1;
        counterText.text = $"#{counter}";
        PlayerPrefs.SetInt("counter", 1);
        PlayerPrefs.Save();
    }
}
