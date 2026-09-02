using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.InputSystem;

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

    [Header("Player References")]
    [SerializeField]
    private GameObject Player1;

    [SerializeField]
    private GameObject Player2;

    [SerializeField]
    private GameObject Ball;

    public PlayerInputManager _playerInputManager;

    private bool checkPlayerCount;

    private bool matchOver;

    public bool IsMatchOver =>
        matchOver;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Debug.LogWarning(
                "[ScoreManager] Duplicate ScoreManager found. " +
                "Destroying duplicate."
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

    private void Update()
    {
        if (checkPlayerCount)
            return;

        if (_playerInputManager == null)
            return;

        if (_playerInputManager.playerCount >= 2)
        {
            AssignPlayers();
        }
    }

    private void AssignPlayers()
    {
        GameObject player1 =
            GameObject.FindGameObjectWithTag(
                Player1Tag
            );

        GameObject player2 =
            GameObject.FindGameObjectWithTag(
                Player2Tag
            );

        GameObject ball =
            GameObject.FindGameObjectWithTag(
                "Ball"
            );

        // Only mark the players as assigned
        // when both actually exist.
        if (player1 == null ||
            player2 == null)
        {
            Debug.LogWarning(
                "[ScoreManager] Two players have joined, " +
                "but Player1/Player2 could not both be found yet."
            );

            return;
        }

        Player1 = player1;
        Player2 = player2;
        Ball = ball;

        checkPlayerCount = true;

        Debug.Log(
            "[ScoreManager] Players assigned successfully."
        );
    }

    public void OnBallEnteredGoal(
        string goalOwnerTag)
    {
        Debug.Log(
            "[ScoreManager] Ball entered " +
            goalOwnerTag +
            "'s goal."
        );

        if (matchOver)
        {
            Debug.Log(
                "[ScoreManager] Match is already over. " +
                "Ignoring goal."
            );

            return;
        }

        string scoringPlayerTag;

        // Player 1's goal was entered.
        // Therefore Player 2 scores.
        if (goalOwnerTag == Player1Tag)
        {
            scoringPlayerTag =
                Player2Tag;
        }

        // Player 2's goal was entered.
        // Therefore Player 1 scores.
        else if (goalOwnerTag == Player2Tag)
        {
            scoringPlayerTag =
                Player1Tag;
        }
        else
        {
            Debug.LogWarning(
                "[ScoreManager] Invalid goal owner tag: " +
                goalOwnerTag
            );

            return;
        }

        // Explode the player who conceded.
        PlayExplosionOnPlayer(
            goalOwnerTag
        );

        onPlayerScoredOn?.Invoke(
            goalOwnerTag
        );

        // Add one point.
        AddPoint(
            scoringPlayerTag
        );
    }

    private void PlayExplosionOnPlayer(
        string playerTag)
    {
        GameObject player =
            GameObject.FindGameObjectWithTag(
                playerTag
            );

        if (player == null)
        {
            Debug.LogWarning(
                "[ScoreManager] Could not find player with tag '" +
                playerTag +
                "'."
            );

            return;
        }

        AnimationManager animationManager =
            player.GetComponent<AnimationManager>();

        if (animationManager == null)
        {
            Debug.LogWarning(
                "[ScoreManager] Player '" +
                playerTag +
                "' does not have an AnimationManager."
            );

            return;
        }

        animationManager.PlayExplotion();

        Debug.Log(
            "[ScoreManager] Explosion animation triggered on " +
            playerTag
        );
    }

    public void AddPoint(
        string scoringPlayerTag)
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
                "[ScoreManager] Unrecognized player tag: " +
                scoringPlayerTag
            );

            return;
        }

        UpdateScoreUI();

        Debug.Log(
            "[ScoreManager] Point awarded to " +
            scoringPlayerTag
        );

        Debug.Log(
            "[ScoreManager] SCORE: " +
            "Player 1 = " +
            player1Score +
            " | Player 2 = " +
            player2Score
        );

        // Tell anything listening that a goal happened.
        onGoalScored?.Invoke(
            scoringPlayerTag
        );

        // IMPORTANT:
        // There is NO winner check here.
        //
        // The score does NOT end the game.
        // The timer is the ONLY thing that ends the match.

        ResetBallImmediate();

        RespawnPlayersIfAssigned();
    }

    public int GetScore(
        string playerTag)
    {
        if (playerTag == Player1Tag)
            return player1Score;

        if (playerTag == Player2Tag)
            return player2Score;

        return 0;
    }

    public void EndMatchByTimeout()
    {
        if (matchOver)
            return;

        matchOver = true;

        string result;

        // Player 1 has more points.
        if (player1Score > player2Score)
        {
            result = Player1Tag;

            RemoveLoserFromCamera(
                Player2
            );

            RemoveBallFromCamera();
        }

        // Player 2 has more points.
        else if (player2Score > player1Score)
        {
            result = Player2Tag;

            RemoveLoserFromCamera(
                Player1
            );

            RemoveBallFromCamera();
        }

        // Same score.
        else
        {
            result = "Draw";
        }

        Debug.Log(
            "[ScoreManager] TIME'S UP!"
        );

        Debug.Log(
            "[ScoreManager] Final Score: " +
            "Player 1 = " +
            player1Score +
            " | Player 2 = " +
            player2Score
        );

        Debug.Log(
            "[ScoreManager] Result: " +
            result
        );

        // This is the ONLY place where
        // the winner is determined.
        onMatchWon?.Invoke(
            result
        );
    }

    private void RemoveLoserFromCamera(
        GameObject player)
    {
        if (player == null)
            return;

        TargetGroupAutoRegister cameraRegister =
            player.GetComponent<TargetGroupAutoRegister>();

        if (cameraRegister != null)
        {
            cameraRegister.RemovePlayer();

            Debug.Log(
                "[ScoreManager] Removed losing player " +
                "from camera target group."
            );
        }
    }

    private void RemoveBallFromCamera()
    {
        if (Ball == null)
        {
            Ball =
                GameObject.FindGameObjectWithTag(
                    "Ball"
                );
        }

        if (Ball == null)
            return;

        TargetGroupAutoRegister ballRegister =
            Ball.GetComponent<TargetGroupAutoRegister>();

        if (ballRegister != null)
        {
            ballRegister.RemovePlayer();

            Debug.Log(
                "[ScoreManager] Removed ball " +
                "from camera target group."
            );
        }
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
                "[ScoreManager] Ball Rigidbody is not assigned."
            );

            return;
        }

        ballRigidbody.linearVelocity =
            Vector3.zero;

        ballRigidbody.angularVelocity =
            Vector3.zero;

        if (ballSpawnPoint != null)
        {
            ballRigidbody.transform
                .SetPositionAndRotation(
                    ballSpawnPoint.position,
                    ballSpawnPoint.rotation
                );
        }
        else
        {
            Debug.LogWarning(
                "[ScoreManager] Ball Spawn Point is not assigned. " +
                "Only the ball velocity was reset."
            );
        }
    }

    private void UpdateScoreUI()
    {
        if (player1ScoreText != null)
        {
            player1ScoreText.text =
                player1Score.ToString();
        }

        if (player2ScoreText != null)
        {
            player2ScoreText.text =
                player2Score.ToString();
        }

        Debug.Log(
            "[ScoreManager] SCORE: " +
            player1Score +
            " - " +
            player2Score
        );
    }
}

