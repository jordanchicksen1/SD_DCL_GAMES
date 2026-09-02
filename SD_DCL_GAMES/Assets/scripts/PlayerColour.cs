using UnityEngine;

public class PlayerColour : MonoBehaviour
{
    [Header("Mesh To Recolour")]
    [SerializeField] private Renderer playerMesh;

    [Header("Player Materials")]
    [SerializeField] private Material player1Material;
    [SerializeField] private Material player2Material;

    public void SetPlayerColour(string playerTag)
    {
        if (playerMesh == null)
        {
            Debug.LogWarning(
                "[PlayerColour] Player mesh is not assigned."
            );
            return;
        }

        if (playerTag == "Player1")
        {
            playerMesh.material = player1Material;
        }
        else if (playerTag == "Player2")
        {
            playerMesh.material = player2Material;
        }
        else
        {
            Debug.LogWarning(
                "[PlayerColour] Unknown player tag: " + playerTag
            );
        }
    }
}