using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public const string Player1Tag = "Player1";
    public const string Player2Tag = "Player2";

    [Header("UI")]
    [SerializeField]
    private TMP_Text player1ScoreText;

    [SerializeField]
    private TMP_Text player2ScoreText;

    [Header("Match Settings")]
    [SerializeField]
    private int pointsToWin = 5;

    [Header("Ball Reset")]
    [SerializeField]
    private Rigidbody ballRigidbody;

    [SerializeField]
    private Transform ballSpawnPoint;

    [Header("Player Respawn")]
    [SerializeField]
    private RespawnManager respawnManager;

    [Header("Events")]

    public UnityEvent<string> onGoalScored;

    public UnityEvent<string> onPlayerScoredOn;

    public UnityEvent<string> onMatchWon;

    private int player1Score;
    private int player2Score;

    private bool matchOver;

    public bool IsMatchOver => matchOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "[ScoreManager] Duplicate ScoreManager found. Destroying duplicate."
            );

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
        Debug.Log(
            $"[ScoreManager] Ball entered {goalOwnerTag}'s goal."
        );

        if (matchOver)
        {
            Debug.Log(
                "[ScoreManager] Match is already over - ignoring goal."
            );

            return;
        }

        string scoringPlayerTag;

        if (goalOwnerTag == Player1Tag)
        {
            scoringPlayerTag = Player2Tag;
        }
        else if (goalOwnerTag == Player2Tag)
        {
            scoringPlayerTag = Player1Tag;
        }
        else
        {
            Debug.LogWarning(
                $"[ScoreManager] Invalid goal owner tag: {goalOwnerTag}. " +
                "Expected Player1 or Player2."
            );

            return;
        }

        PlayExplosionOnPlayer(goalOwnerTag);

        onPlayerScoredOn?.Invoke(goalOwnerTag);

        AddPoint(scoringPlayerTag);
    }

    private void PlayExplosionOnPlayer(string playerTag)
    {
        GameObject player =
            GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            Debug.LogWarning(
                $"[ScoreManager] Could not find player with tag '{playerTag}'. " +
                "Make sure the player has spawned and has the correct tag."
            );

            return;
        }

        AnimationManager animationManager =
            player.GetComponent<AnimationManager>();

        if (animationManager == null)
        {
            Debug.LogWarning(
                $"[ScoreManager] Player '{playerTag}' does not have " +
                "an AnimationManager component."
            );

            return;
        }

        animationManager.PlayExplotion();

        Debug.Log(
            $"[ScoreManager] Explosion animation triggered on {playerTag}."
        );
    }

    public void AddPoint(string scoringPlayerTag)
    {
        if (matchOver)
            return;

        if (scoringPlayerTag == Player1Tag)
        {
            player1Score++;
        }
        else if (scoringPlayerTag == Player2Tag)
        {
            player2Score++;
        }
        else
        {
            Debug.LogWarning(
                $"[ScoreManager] Unrecognized player tag " +
                $"'{scoringPlayerTag}'. Expected Player1 or Player2."
            );

            return;
        }

        UpdateScoreUI();


        Debug.Log(
            $"[ScoreManager] Point awarded to {scoringPlayerTag}."
        );

        Debug.Log(
            $"[ScoreManager] Score: " +
            $"Player 1 = {player1Score} | " +
            $"Player 2 = {player2Score}"
        );

        onGoalScored?.Invoke(scoringPlayerTag);

        if (
            pointsToWin > 0 &&
            GetScore(scoringPlayerTag) >= pointsToWin
        )
        {
            matchOver = true;

            Debug.Log(
                $"[ScoreManager] {scoringPlayerTag} wins the match!"
            );

            onMatchWon?.Invoke(scoringPlayerTag);

            return;
        }

        ResetBallImmediate();

        RespawnPlayersIfAssigned();
    }

    public int GetScore(string playerTag)
    {
        if (playerTag == Player1Tag)
        {
            return player1Score;
        }

        if (playerTag == Player2Tag)
        {
            return player2Score;
        }

        return 0;
    }

    public void EndMatchByTimeout()
    {
        if (matchOver)
            return;

        matchOver = true;

        string result;

        if (player1Score > player2Score)
        {
            result = Player1Tag;
        }
        else if (player2Score > player1Score)
        {
            result = Player2Tag;
        }
        else
        {
            result = "Draw";
        }


        Debug.Log(
            $"[ScoreManager] Time's up! " +
            $"Player 1: {player1Score} | " +
            $"Player 2: {player2Score} | " +
            $"Result: {result}"
        );

        onMatchWon?.Invoke(result);
    }

    public void ResetMatch()
    {
        player1Score = 0;
        player2Score = 0;

        matchOver = false;

        UpdateScoreUI();

        ResetBallImmediate();

        RespawnPlayersIfAssigned();

        Debug.Log(
            "[ScoreManager] Match reset."
        );
    }

    private void RespawnPlayersIfAssigned()
    {
        if (respawnManager == null)
        {
            Debug.LogWarning(
                "[ScoreManager] Respawn Manager is not assigned. " +
                "Drag your RespawnManager into the ScoreManager Inspector."
            );

            return;
        }

        respawnManager.RespawnPlayers();
    }

    private void ResetBallImmediate()
    {
        if (ballRigidbody == null)
        {
            Debug.LogWarning(
                "[ScoreManager] Ball Rigidbody is not assigned. " +
                "Drag the ball Rigidbody into the Inspector."
            );

            return;
        }


        ballRigidbody.linearVelocity = Vector3.zero;

        ballRigidbody.angularVelocity = Vector3.zero;

        if (ballSpawnPoint != null)
        {
            ballRigidbody.transform.SetPositionAndRotation(
                ballSpawnPoint.position,
                ballSpawnPoint.rotation
            );

            Debug.Log(
                $"[ScoreManager] Ball reset to " +
                $"{ballSpawnPoint.position}."
            );
        }
        else
        {
            Debug.LogWarning(
                "[ScoreManager] Ball Spawn Point is not assigned. " +
                "Ball velocity was reset, but its position was not changed."
            );
        }
    }


    private void UpdateScoreUI()
    {
        Debug.Log(
            $"[ScoreManager] Score is now " +
            $"Player 1: {player1Score} | " +
            $"Player 2: {player2Score}"
        );


        if (player1ScoreText != null)
        {
            player1ScoreText.text =
                player1Score.ToString();
        }
        else
        {
            Debug.LogWarning(
                "[ScoreManager] Player 1 Score Text is not assigned."
            );
        }


        if (player2ScoreText != null)
        {
            player2ScoreText.text =
                player2Score.ToString();
        }
        else
        {
            Debug.LogWarning(
                "[ScoreManager] Player 2 Score Text is not assigned."
            );
        }
    }
}
