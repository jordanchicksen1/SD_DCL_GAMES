using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Match length in seconds. Default is 3 minutes (180s).")]
    [SerializeField] private float matchDuration = 180f;

    [Header("UI (optional)")]
    [SerializeField] private TMP_Text timerText;

    [Header("Events (optional)")]
    [Tooltip("Fired once, the instant the timer reaches 0.")]
    public UnityEvent onTimerEnded;

    private float timeRemaining;
    private bool isRunning;
    private int lastLoggedSecond = -1;

    private void Start()
    {
        timeRemaining = matchDuration;
        isRunning = true;
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (ScoreManager.Instance != null && ScoreManager.Instance.IsMatchOver)
        {
            isRunning = false;
            Debug.Log("[GameTimer] Match already ended (by points) - stopping timer.");
            return;
        }

        timeRemaining -= Time.deltaTime;

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
        Debug.Log("[GameTimer] Time's up!");
        onTimerEnded?.Invoke();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.EndMatchByTimeout();
        else
            Debug.LogWarning("[GameTimer] No ScoreManager found in the scene - can't determine a winner.");
    }

    public void PauseTimer() => isRunning = false;

    public void ResumeTimer()
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.IsMatchOver)
            return;

        isRunning = true;
    }

    public void ResetTimer()
    {
        timeRemaining = matchDuration;
        isRunning = true;
        lastLoggedSecond = -1;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        string formatted = $"{minutes:00}:{seconds:00}";

        if (timerText != null)
            timerText.text = formatted;

        int wholeSecond = Mathf.CeilToInt(timeRemaining);
        if (wholeSecond != lastLoggedSecond)
        {
            lastLoggedSecond = wholeSecond;
            Debug.Log($"[GameTimer] Time remaining: {formatted}");
        }
    }
}