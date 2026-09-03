using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    [Header("Spawn Points")]

    [Tooltip("Where Player 1 starts and respawns.")]
    [SerializeField]
    private Transform player1SpawnPoint;

    [Tooltip("Where Player 2 starts and respawns.")]
    [SerializeField]
    private Transform player2SpawnPoint;


    [Header("Respawn Settings")]

    [Tooltip("Time to wait before respawning players.")]
    [SerializeField]
    private float respawnDelay = 0f;

    [Header("Match Start Countdown")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float countdownTime = 1f;

    private bool countdownStarted;


    private readonly List<Transform> players =
        new List<Transform>();


    public Transform Player1SpawnPoint =>
        player1SpawnPoint;

    public Transform Player2SpawnPoint =>
        player2SpawnPoint;


    // =========================================================
    // ADD PLAYER
    // =========================================================

    public void AddPlayer(Transform player)
    {
        if (player == null)
            return;

        if (players.Contains(player))
            return;


        // -----------------------------------------------------
        // ASSIGN PLAYER TAG
        // -----------------------------------------------------

        if (players.Count == 0)
        {
            SetPlayerTag(player, "Player1");

            PlayerColour playerColour =
                player.GetComponent<PlayerColour>();

            if (playerColour != null)
                playerColour.SetPlayerColour("Player1");

            MovePlayerToSpawn(
                player,
                player1SpawnPoint
            );

            Debug.Log(
                "[RespawnManager] " +
                player.name +
                " assigned as PLAYER 1."
            );
        }
        else if (players.Count == 1)
        {
            SetPlayerTag(player, "Player2");

            PlayerColour playerColour =
                player.GetComponent<PlayerColour>();

            if (playerColour != null)
                playerColour.SetPlayerColour("Player2");

            MovePlayerToSpawn(
                player,
                player2SpawnPoint
            );

            Debug.Log(
                "[RespawnManager] " +
                player.name +
                " assigned as PLAYER 2."
            );
        }
        else
        {
            Debug.LogWarning(
                "[RespawnManager] More than 2 players " +
                "attempted to join the soccer match."
            );

            return;
        }


        // Add player to list.
        players.Add(player);

        if (players.Count == 2 && !countdownStarted)
        {
            StartCoroutine(StartMatchCountdown());
        }


        Debug.Log(
            "[RespawnManager] Registered: " +
            player.name +
            " | Tag: " +
            player.tag
        );
    }

    private System.Collections.IEnumerator StartMatchCountdown()
    {
        countdownStarted = true;

        // Make sure both players cannot move.
        SetPlayersCanMove(false);

        // 3
        countdownText.gameObject.SetActive(true);
        countdownText.text = "3";

        yield return new WaitForSeconds(countdownTime);

        // 2
        countdownText.text = "2";

        yield return new WaitForSeconds(countdownTime);

        // 1
        countdownText.text = "1";

        yield return new WaitForSeconds(countdownTime);

        // START!
        countdownText.text = "START!";

        yield return new WaitForSeconds(0.75f);

        // Let the players play.
        SetPlayersCanMove(true);

        // Hide countdown.
        countdownText.gameObject.SetActive(false);
    }

    private void SetPlayersCanMove(bool canMove)
    {
        CleanupPlayerList();

        foreach (Transform player in players)
        {
            if (player == null)
                continue;

            PlayerController3D controller =
                player.GetComponent<PlayerController3D>();

            if (controller != null)
            {
                controller.SetCanMove(canMove);
            }
        }
    }


    // =========================================================
    // SET PLAYER TAG
    // =========================================================

    private void SetPlayerTag(
        Transform player,
        string tag)
    {
        try
        {
            player.gameObject.tag = tag;
        }
        catch
        {
            Debug.LogError(
                "[RespawnManager] The tag '" +
                tag +
                "' does not exist in Unity's Tags."
            );
        }
    }


    // =========================================================
    // REMOVE PLAYER
    // =========================================================

    public void RemovePlayer(Transform player)
    {
        if (player == null)
            return;

        if (players.Contains(player))
        {
            players.Remove(player);

            Debug.Log(
                "[RespawnManager] Removed player: " +
                player.name
            );
        }
    }


    // =========================================================
    // RESPAWN PLAYERS
    // =========================================================

    public void RespawnPlayers()
    {
        Debug.Log(
            "[RespawnManager] RespawnPlayers() called."
        );


        if (respawnDelay > 0f)
        {
            CancelInvoke(
                nameof(RespawnPlayersImmediate)
            );

            Invoke(
                nameof(RespawnPlayersImmediate),
                respawnDelay
            );
        }
        else
        {
            RespawnPlayersImmediate();
        }
    }


    // =========================================================
    // RESPAWN BOTH PLAYERS
    // =========================================================

    private void RespawnPlayersImmediate()
    {
        CleanupPlayerList();


        Debug.Log(
            "[RespawnManager] Players registered: " +
            players.Count
        );


        for (int i = 0; i < players.Count; i++)
        {
            Transform player = players[i];

            if (player == null)
                continue;


            if (player.CompareTag("Player1"))
            {
                RespawnPlayer(
                    player,
                    player1SpawnPoint
                );
            }
            else if (player.CompareTag("Player2"))
            {
                RespawnPlayer(
                    player,
                    player2SpawnPoint
                );
            }
            else
            {
                Debug.LogWarning(
                    "[RespawnManager] " +
                    player.name +
                    " has an invalid tag: " +
                    player.tag
                );
            }
        }
    }


    // =========================================================
    // RESPAWN INDIVIDUAL PLAYER
    // =========================================================

    private void RespawnPlayer(
        Transform player,
        Transform spawnPoint)
    {
        if (player == null)
            return;


        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "[RespawnManager] Spawn point is missing for " +
                player.name
            );

            return;
        }


        Rigidbody rb =
            player.GetComponent<Rigidbody>();


        if (rb != null)
        {
            // Stop movement.
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;


            // Move Rigidbody.
            rb.position =
                spawnPoint.position;

            rb.rotation =
                spawnPoint.rotation;
        }
        else
        {
            player.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation
            );
        }


        // Reset animation.
        AnimationManager animationManager =
            player.GetComponent<AnimationManager>();

        if (animationManager != null)
        {
            animationManager.PlayIdle();
        }


        Debug.Log(
            "[RespawnManager] SUCCESS: " +
            player.name +
            " respawned at " +
            spawnPoint.name
        );
    }


    // =========================================================
    // MOVE PLAYER TO INITIAL SPAWN
    // =========================================================

    private void MovePlayerToSpawn(
        Transform player,
        Transform spawnPoint)
    {
        if (player == null)
            return;


        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "[RespawnManager] Initial spawn point " +
                "has not been assigned."
            );

            return;
        }


        Rigidbody rb =
            player.GetComponent<Rigidbody>();


        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position =
                spawnPoint.position;

            rb.rotation =
                spawnPoint.rotation;
        }
        else
        {
            player.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation
            );
        }


        Debug.Log(
            "[RespawnManager] " +
            player.name +
            " moved to initial spawn: " +
            spawnPoint.name
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void CleanupPlayerList()
    {
        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
            }
        }
    }


    // =========================================================
    // DEBUG GIZMOS
    // =========================================================

    private void OnDrawGizmos()
    {
        if (player1SpawnPoint != null)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawSphere(
                player1SpawnPoint.position,
                0.25f
            );

            Gizmos.DrawLine(
                player1SpawnPoint.position,
                player1SpawnPoint.position +
                player1SpawnPoint.forward
            );
        }


        if (player2SpawnPoint != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(
                player2SpawnPoint.position,
                0.25f
            );

            Gizmos.DrawLine(
                player2SpawnPoint.position,
                player2SpawnPoint.position +
                player2SpawnPoint.forward
            );
        }
    }
}
