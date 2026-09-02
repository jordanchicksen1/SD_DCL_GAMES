using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalPost : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball"))
            return;

        if (ScoreManager.Instance == null)
        {
            Debug.LogWarning($"GoalPost ({name}): ball entered but no ScoreManager was found in the scene.");
            return;
        }

        string ownerTag;
        if (CompareTag("P1Goal"))
            ownerTag = ScoreManager.Player1Tag;
        else if (CompareTag("P2Goal"))
            ownerTag = ScoreManager.Player2Tag;
        else
        {
            Debug.LogWarning($"GoalPost ({name}): tag it \"P1Goal\" or \"P2Goal\" so it knows which player defends it.");
            return;
        }

        ScoreManager.Instance.OnBallEnteredGoal(ownerTag);
    }
}