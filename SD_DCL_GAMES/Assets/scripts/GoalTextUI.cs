using UnityEngine;

public class GoalTextUI : MonoBehaviour
{
    [Header("Goal Texts")]
    [SerializeField] private GameObject goalTextP1;
    [SerializeField] private GameObject goalTextP2;

    [Header("Display Settings")]
    [SerializeField] private float displayTime = 2f;

    private Coroutine hideCoroutine;

    private void Start()
    {
        // Make sure both messages are hidden when the game starts.
        goalTextP1.SetActive(false);
        goalTextP2.SetActive(false);
    }

    public void ShowGoal(string scoringPlayerTag)
    {
        // Hide both first.
        goalTextP1.SetActive(false);
        goalTextP2.SetActive(false);

        // Show the correct message.
        if (scoringPlayerTag == ScoreManager.Player1Tag)
        {
            goalTextP1.SetActive(true);
        }
        else if (scoringPlayerTag == ScoreManager.Player2Tag)
        {
            goalTextP2.SetActive(true);
        }

        // Restart the hide timer.
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideGoalAfterDelay());
    }

    private System.Collections.IEnumerator HideGoalAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        goalTextP1.SetActive(false);
        goalTextP2.SetActive(false);

        hideCoroutine = null;
    }

    public void HideGoal()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        goalTextP1.SetActive(false);
        goalTextP2.SetActive(false);
    }
}