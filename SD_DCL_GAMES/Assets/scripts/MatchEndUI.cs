
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MatchEndUI : MonoBehaviour
{
    [Header("Match End Buttons")]
    [SerializeField]
    private Button replayButton;

    [SerializeField]
    private Button quitButton;

    [Header("Pause")]
    [Tooltip("Panel that appears when the game is paused.")]
    [SerializeField]
    private GameObject pausePanel;

    [Tooltip("First button selected when the pause panel opens.")]
    [SerializeField]
    private Button firstPauseButton;

    [Header("Scene")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        // Make sure pause panel starts hidden.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Make sure the game starts running.
        Time.timeScale = 1f;

        // Select Replay for the match-end screen.
        SelectButton(replayButton);
    }

    private void Update()
    {
        // Check for controller Options / Start button.
        if (Gamepad.current != null &&
            Gamepad.current.startButton.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    // =========================================================
    // PAUSE
    // =========================================================

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

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // Select the first pause button.
        SelectButton(firstPauseButton);

        Debug.Log("[MatchEndUI] Game paused.");
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Debug.Log("[MatchEndUI] Game resumed.");
    }

    // =========================================================
    // REPLAY
    // =========================================================

    public void Replay()
    {
        Debug.Log("[MatchEndUI] Replay clicked.");

        // Always restore time before loading another scene.
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.name
        );
    }

    // =========================================================
    // QUIT TO MAIN MENU
    // =========================================================

    public void QuitToMainMenu()
    {
        Debug.Log(
            "[MatchEndUI] Quit clicked. Loading MainMenu."
        );

        // Restore time before changing scene.
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    // =========================================================
    // CONTROLLER SELECTION
    // =========================================================

    private void SelectButton(Button button)
    {
        if (button == null)
            return;

        if (EventSystem.current == null)
        {
            Debug.LogWarning(
                "[MatchEndUI] No EventSystem found in the scene."
            );

            return;
        }

        EventSystem.current.SetSelectedGameObject(
            null
        );

        EventSystem.current.SetSelectedGameObject(
            button.gameObject
        );

        button.Select();
    }

    // =========================================================
    // OPTIONAL: PAUSE BUTTON
    // =========================================================

    // You can connect a normal UI button to this
    // if you also want a pause button on screen.
    public void OnPauseButtonPressed()
    {
        TogglePause();
    }

    // =========================================================
    // OPTIONAL: RESUME BUTTON
    // =========================================================

    public void OnResumeButtonPressed()
    {
        ResumeGame();
    }

    private void OnDestroy()
    {
        // Prevent the next scene from accidentally
        // remaining paused.
        Time.timeScale = 1f;
    }
}
