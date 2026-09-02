using UnityEngine;
using TMPro;

public class WinPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winText;

    [Header("Messages")]
    [SerializeField] private string player1WinMessage = "Player 1 Wins!";
    [SerializeField] private string player2WinMessage = "Player 2 Wins!";
    [SerializeField] private string drawMessage = "It's a Draw!";

    private void Start()
    {
        // Hide the panel when the match starts.
        if (winPanel != null)
            winPanel.SetActive(false);

        // Listen for the match ending.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.onMatchWon.AddListener(ShowWinPanel);
        }
        else
        {
            Debug.LogError("[WinPanelUI] ScoreManager.Instance was not found.");
        }
    }

    public void ShowWinPanel(string result)
    {
        Debug.Log($"[WinPanelUI] RECEIVED RESULT: '{result}'");

        string message;

        if (result == ScoreManager.Player1Tag)
        {
            message = player1WinMessage;
        }
        else if (result == ScoreManager.Player2Tag)
        {
            message = player2WinMessage;
        }
        else if (result == "Draw")
        {
            message = drawMessage;
        }
        else
        {
            message = $"UNKNOWN RESULT: {result}";
        }

        Debug.Log($"[WinPanelUI] FINAL MESSAGE: {message}");

        if (winText != null)
        {
            winText.text = message;
        }
        else
        {
            Debug.LogWarning("[WinPanelUI] Win Text is not assigned.");
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[WinPanelUI] Win Panel is not assigned.");
        }
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.onMatchWon.RemoveListener(ShowWinPanel);
        }
    }
}