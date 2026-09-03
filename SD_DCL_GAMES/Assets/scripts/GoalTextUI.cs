using UnityEngine;

public class GoalTextUI : MonoBehaviour
{
    [Header("Goal Texts")]
    [SerializeField] private GameObject goalTextP1;
    [SerializeField] private GameObject goalTextP2;

    [Header("Goal Particle Effects")]
    [SerializeField] private ParticleSystem goalEffectP1;
    [SerializeField] private ParticleSystem goalEffectP2;

    [Header("Display Settings")]
    [SerializeField] private float displayTime = 2f;

    private Coroutine hideCoroutine;

    private void Start()
    {
        // Hide both goal messages when the game starts.
        goalTextP1.SetActive(false);
        goalTextP2.SetActive(false);

        // Listen for goals from the ScoreManager.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.onGoalScored.AddListener(ShowGoal);
        }
        else
        {
            Debug.LogError(
                "[GoalTextUI] ScoreManager.Instance was not found."
            );
        }
    }

    public void ShowGoal(string scoringPlayerTag)
    {
        Debug.Log(
            "[GoalTextUI] Goal received from: " +
            scoringPlayerTag
        );

        // Hide both texts first.
        goalTextP1.SetActive(false);
        goalTextP2.SetActive(false);

        // Player 1 scored.
        if (scoringPlayerTag == ScoreManager.Player1Tag)
        {
            goalTextP1.SetActive(true);

            if (goalEffectP1 != null)
            {
                goalEffectP1.Play();
            }
        }

        // Player 2 scored.
        else if (scoringPlayerTag == ScoreManager.Player2Tag)
        {
            goalTextP2.SetActive(true);

            if (goalEffectP2 != null)
            {
                goalEffectP2.Play();
            }
        }
        else
        {
            Debug.LogWarning(
                "[GoalTextUI] Unknown scoring player: " +
                scoringPlayerTag
            );

            return;
        }

        // Restart the hide timer.
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(
            HideGoalAfterDelay()
        );
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

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.onGoalScored.RemoveListener(
                ShowGoal
            );
        }
    }
}