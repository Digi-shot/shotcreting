using UnityEngine;

public class PauseAfterW : MonoBehaviour
{
    public float delayAfterW = 10f;

    private bool countdownStarted = false;
    private bool hasPaused = false;
    private float timer = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (!countdownStarted && Input.GetKeyDown(KeyCode.W))
        {
            countdownStarted = true;
            timer = 0f;
        }

        if (countdownStarted && !hasPaused)
        {
            timer += Time.deltaTime;

            if (timer >= delayAfterW)
            {
                Time.timeScale = 0f;
                hasPaused = true;
            }
        }
    }
}