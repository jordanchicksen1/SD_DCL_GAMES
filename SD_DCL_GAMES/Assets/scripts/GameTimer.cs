
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Match length in seconds. Default is 3 minutes.")]
    [SerializeField]
    private float matchDuration = 180f;

    [Header("UI")]
    [SerializeField]
    private TMP_Text timerText;

    [Header("Events")]
    [Tooltip("Fired once when the timer reaches 0.")]
    public UnityEvent onTimerEnded;

    private float timeRemaining;

    private bool isRunning;

    private int lastLoggedSecond = -1;

    private void Start()
    {
        timeRemaining =
            matchDuration;

        isRunning = true;

        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        timeRemaining -=
            Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;

            isRunning = false;

            UpdateTimerDisplay();

            HandleTimerEnded();

            return;
        }

        UpdateTimerDisplay();
    }

    private void HandleTimerEnded()
    {
        Debug.Log(
            "[GameTimer] TIME'S UP!"
        );

        // Fire the Unity Event first.
        onTimerEnded?.Invoke();

        // Tell ScoreManager to determine
        // who won based on the final score.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance
                .EndMatchByTimeout();
        }
        else
        {
            Debug.LogWarning(
                "[GameTimer] No ScoreManager found. " +
                "Cannot determine winner."
            );
        }
    }

    public void PauseTimer()
    {
        isRunning = false;

        Debug.Log(
            "[GameTimer] Timer paused."
        );
    }

    public void ResumeTimer()
    {
        if (timeRemaining <= 0f)
            return;

        isRunning = true;

        Debug.Log(
            "[GameTimer] Timer resumed."
        );
    }

    public void ResetTimer()
    {
        timeRemaining =
            matchDuration;

        isRunning = true;

        lastLoggedSecond = -1;

        UpdateTimerDisplay();

        Debug.Log(
            "[GameTimer] Timer reset."
        );
    }

    private void UpdateTimerDisplay()
    {
        int minutes =
            Mathf.FloorToInt(
                timeRemaining / 60f
            );

        int seconds =
            Mathf.FloorToInt(
                timeRemaining % 60f
            );

        string formatted =
            $"{minutes:00}:{seconds:00}";

        if (timerText != null)
        {
            timerText.text =
                formatted;
        }

        int wholeSecond =
            Mathf.CeilToInt(
                timeRemaining
            );

        if (wholeSecond !=
            lastLoggedSecond)
        {
            lastLoggedSecond =
                wholeSecond;

            Debug.Log(
                "[GameTimer] Time remaining: " +
                formatted
            );
        }
    }
}

