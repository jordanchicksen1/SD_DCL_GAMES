using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public const string Player1Tag = "Player1";
    public const string Player2Tag = "Player2";

    [Header("UI")]
    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;

    [Header("Match Settings")]
    [SerializeField] private int pointsToWin = 5;

    [Header("Ball Reset (optional)")]
    [SerializeField] private Rigidbody ballRigidbody;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private float goalResetDelay = 1.5f;

    [Header("Events (optional)")]
    public UnityEvent<string> onGoalScored;
    public UnityEvent<string> onMatchWon;

    private int player1Score;
    private int player2Score;
    private bool matchOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate ScoreManager found - destroying the extra one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void OnBallEnteredGoal(string goalOwnerTag)
    {
        if (matchOver)
            return;

        string scoringPlayerTag = goalOwnerTag == Player1Tag ? Player2Tag : Player1Tag;
        AddPoint(scoringPlayerTag);
    }

    public void AddPoint(string scoringPlayerTag)
    {
        if (matchOver)
            return;

        if (scoringPlayerTag == Player1Tag)
            player1Score++;
        else if (scoringPlayerTag == Player2Tag)
            player2Score++;
        else
        {
            Debug.LogWarning($"AddPoint: unrecognized player tag \"{scoringPlayerTag}\" - expected \"Player1\" or \"Player2\".");
            return;
        }

        UpdateScoreUI();
        onGoalScored?.Invoke(scoringPlayerTag);

        if (pointsToWin > 0 && GetScore(scoringPlayerTag) >= pointsToWin)
        {
            matchOver = true;
            onMatchWon?.Invoke(scoringPlayerTag);
            return; 
        }

        if (ballRigidbody != null)
            StartCoroutine(ResetBallAfterDelay());
    }

    public int GetScore(string playerTag) => playerTag == Player1Tag ? player1Score : player2Score;

    public void ResetMatch()
    {
        player1Score = 0;
        player2Score = 0;
        matchOver = false;
        UpdateScoreUI();
        ResetBallImmediate();
    }

    private IEnumerator ResetBallAfterDelay()
    {
        yield return new WaitForSeconds(goalResetDelay);
        ResetBallImmediate();
    }

    private void ResetBallImmediate()
    {
        if (ballRigidbody == null)
            return;

        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        if (ballSpawnPoint != null)
            ballRigidbody.transform.SetPositionAndRotation(ballSpawnPoint.position, ballSpawnPoint.rotation);
    }

    private void UpdateScoreUI()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = player1Score.ToString();

        if (player2ScoreText != null)
            player2ScoreText.text = player2Score.ToString();
    }
}