using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Exact name of the gameplay scene as it appears in Build Settings.")]
    [SerializeField] private string gameSceneName = "MatchScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainButtonsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject quitConfirmPanel;

    [Header("Keyboard Navigation")]
    [Tooltip("The button that gets highlighted/selected by default on the main panel (e.g. the Play button).")]
    [SerializeField] private GameObject firstSelectedOnMainPanel;
    [Tooltip("The button that gets highlighted/selected when the Controls panel opens (usually its Back button).")]
    [SerializeField] private GameObject firstSelectedOnControlsPanel;
    [Tooltip("The button that gets highlighted/selected when the Quit confirm panel opens (usually 'Cancel', so an accidental press doesn't quit).")]
    [SerializeField] private GameObject firstSelectedOnQuitPanel;

    [Header("Optional: Loading Screen")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingBar;

    [Header("Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip buttonClickSfx;

    private GameObject currentPanel;

    private void Start()
    {
        ShowOnly(mainButtonsPanel, firstSelectedOnMainPanel);
    }

    private void Update()
    {
        HandleCancelInput();
        HandleReselectAfterMouseUse();
    }

    public void OnPlayPressed()
    {
        PlayClickSfx();
        StartCoroutine(LoadGameSceneAsync());
    }

    public void OnQuitPressed()
    {
        PlayClickSfx();
        ShowOnly(quitConfirmPanel, firstSelectedOnQuitPanel);
    }

    public void ConfirmQuit()
    {
        PlayClickSfx();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void CancelQuit()
    {
        PlayClickSfx();
        ShowOnly(mainButtonsPanel, firstSelectedOnMainPanel);
    }

    public void OnControlsPressed()
    {
        PlayClickSfx();
        ShowOnly(controlsPanel, firstSelectedOnControlsPanel);
    }

    public void OnBackPressed()
    {
        PlayClickSfx();
        ShowOnly(mainButtonsPanel, firstSelectedOnMainPanel);
    }

    private IEnumerator LoadGameSceneAsync()
    {
        if (loadingPanel != null)
        {
            mainButtonsPanel.SetActive(false);
            loadingPanel.SetActive(true);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (loadingBar != null)
                loadingBar.value = op.progress;
            yield return null;
        }

        if (loadingBar != null)
            loadingBar.value = 1f;

        yield return new WaitForSeconds(0.25f); 
        op.allowSceneActivation = true;
    }

    private void ShowOnly(GameObject panelToShow, GameObject firstSelected)
    {
        GameObject[] allPanels = { mainButtonsPanel, controlsPanel, quitConfirmPanel };
        foreach (var panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(panel == panelToShow);
        }

        currentPanel = panelToShow;
        SelectUIElement(firstSelected);
    }

    private void SelectUIElement(GameObject uiElement)
    {
        if (uiElement == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null); 
        EventSystem.current.SetSelectedGameObject(uiElement);
    }

    private void HandleCancelInput()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (currentPanel == controlsPanel)
            OnBackPressed();
        else if (currentPanel == quitConfirmPanel)
            CancelQuit();
    }

    private void HandleReselectAfterMouseUse()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null && currentPanel != null)
        {
            GameObject fallback = currentPanel == mainButtonsPanel ? firstSelectedOnMainPanel
                                 : currentPanel == controlsPanel ? firstSelectedOnControlsPanel
                                 : firstSelectedOnQuitPanel;

            SelectUIElement(fallback);
        }
    }

    private void PlayClickSfx()
    {
        if (uiAudioSource != null && buttonClickSfx != null)
            uiAudioSource.PlayOneShot(buttonClickSfx);
    }
}