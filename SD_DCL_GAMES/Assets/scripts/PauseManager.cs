
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Panel")]
    [SerializeField]
    private GameObject pausePanel;

    [Header("Main Menu")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        // Make sure the pause panel is hidden
        // when the game starts.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Update()
    {
        if (Gamepad.current == null)
            return;

        // PlayStation / Xbox / generic controller
        // Start / Options / Menu button.
        if (Gamepad.current.startButton.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Debug.Log("[PauseManager] PAUSED");
    }

    private void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Debug.Log("[PauseManager] RESUMED");
    }

    public void ExitToMainMenu()
    {
        Debug.Log("[PauseManager] Loading MainMenu");

        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}

