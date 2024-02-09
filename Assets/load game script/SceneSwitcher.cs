using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public float timeBeforeSceneChange = 60f; // Time before changing scenes

    private float timer = 0f;
    private bool sceneSwitched = false;

    void Update()
    {
        // Increment the timer
        timer += Time.deltaTime;

        // Check if the timer exceeds the specified time and the scene hasn't been switched yet
        if (timer >= timeBeforeSceneChange && !sceneSwitched)
        {
            // Call a function to switch the scene
            SwitchScene();
            sceneSwitched = true; // Set the flag to prevent multiple scene switches
        }
    }

    void SwitchScene()
    {
        // Load the next scene by its name ("scene2" in this case)
        SceneManager.LoadScene("pierwszaTestowaPlansza");
    }
}