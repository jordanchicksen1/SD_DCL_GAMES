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
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void ShowWinPanel(string result)
    {
        string message = result switch
        {
            ScoreManager.Player1Tag => player1WinMessage,
            ScoreManager.Player2Tag => player2WinMessage,
            _ => drawMessage
        };

        Debug.Log($"[WinPanelUI] Match ended - {message}");

        if (winText != null)
            winText.text = message;
        else
            Debug.LogWarning("[WinPanelUI] Win Text is not assigned - message only visible in Console.");

        if (winPanel != null)
            winPanel.SetActive(true);
        else
            Debug.LogWarning("[WinPanelUI] Win Panel is not assigned - nothing to show.");
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }
}