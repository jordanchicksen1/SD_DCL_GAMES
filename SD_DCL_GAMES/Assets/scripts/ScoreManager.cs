using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ScoreManager : MonoBehaviour
{    public static ScoreManager Instance { get; private set; }

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
        Debug.Log($"[ScoreManager] Ball entered {goalOwnerTag}'s goal.");

        if (matchOver)
        {
            Debug.Log("[ScoreManager] Match is already over - ignoring goal.");
            return;
        }

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
        Debug.Log($"[ScoreManager] Point awarded to {scoringPlayerTag}.");
        onGoalScored?.Invoke(scoringPlayerTag);

        if (pointsToWin > 0 && GetScore(scoringPlayerTag) >= pointsToWin)
        {
            matchOver = true;
            Debug.Log($"[ScoreManager] {scoringPlayerTag} wins the match!");
            onMatchWon?.Invoke(scoringPlayerTag);
            return; 
        }

        ResetBallImmediate();
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

    private void ResetBallImmediate()
    {
        if (ballRigidbody == null)
        {
            Debug.LogWarning("[ScoreManager] Ball Rigidbody is not assigned - skipping ball reset. Drag the ball's Rigidbody into the ScoreManager Inspector.");
            return;
        }

        if (ballSpawnPoint == null)
        {
            Debug.LogWarning("[ScoreManager] Ball Spawn Point is not assigned - resetting velocity only, position won't change. Drag a spawn Transform into the ScoreManager Inspector.");
        }

        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        if (ballSpawnPoint != null)
        {
            ballRigidbody.transform.SetPositionAndRotation(ballSpawnPoint.position, ballSpawnPoint.rotation);
            Debug.Log($"[ScoreManager] Ball reset to spawn point at {ballSpawnPoint.position}.");
        }
    }

    private void UpdateScoreUI()
    {
        Debug.Log($"[ScoreManager] Score is now Player1: {player1Score}  |  Player2: {player2Score}");

        if (player1ScoreText != null)
            player1ScoreText.text = player1Score.ToString();
        else
            Debug.LogWarning("[ScoreManager] Player 1 Score Text is not assigned - score is only visible in the Console for now.");

        if (player2ScoreText != null)
            player2ScoreText.text = player2Score.ToString();
        else
            Debug.LogWarning("[ScoreManager] Player 2 Score Text is not assigned - score is only visible in the Console for now.");
    }
}