using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;
    static public bool GamePaused { get; private set; }

    private LevelCanvas levelCanvas;

    [Header("Pausing")]
    [Tooltip("Allows for the pause menu to be activated, if true")]
    [SerializeField] private bool gameplayScene;
    private GameObject optionsCanvas;
    private GameObject pauseMenu;
    private OptionsMenu optionsMenu;
    [SerializeField] private int mainMenuBuildIndex = 0;

    [Header("Canvas")]
    private GameObject promptCanvas;
    private GameObject conversationCanvas;
    private GameObject dialoguePanel;

    [Header("Mouse")]
    [Tooltip("Locks the cursor in the middle of the screen, if true")]
    [SerializeField] private bool lockCursorWhilePlaying = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        if (gameplayScene) levelCanvas = GameObject.FindGameObjectWithTag("LevelCanvas").GetComponent<LevelCanvas>();
    }

    void Start()
    {
        if (gameplayScene)
        {
            optionsCanvas = levelCanvas.optionsCanvas;
            pauseMenu = levelCanvas.pauseMenu;
            optionsMenu = levelCanvas.optionsMenu;

            promptCanvas = levelCanvas.promptCanvas;
            conversationCanvas = levelCanvas.conversationCanvas;
            dialoguePanel = levelCanvas.dialoguePanel;

            levelCanvas.exitTopperEventTrigger.triggers.Add(new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick,
                callback = new EventTrigger.TriggerEvent()
            });
            levelCanvas.exitTopperEventTrigger.triggers[levelCanvas.exitTopperEventTrigger.triggers.Count - 1].callback.AddListener((e) => ActivatePauseMenu());

            levelCanvas.saveOptionsButton.onClick.AddListener(OptionsManager.instance.SaveSettings);
            levelCanvas.menuButton.onClick.AddListener(LoadMainMenu);
            levelCanvas.settingsButton.onClick.AddListener(DeactivatePauseMenu);
            levelCanvas.unstuckButton.onClick.AddListener(RespawnPlayer);
            levelCanvas.exitButton.onClick.AddListener(ExitGame);
        }

        PreloadCanvas();

        if (lockCursorWhilePlaying && gameplayScene)
            LockCursor();
        else
            UnlockCursor();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && gameplayScene)
        {
            PauseGame();
        }
    }

    // We must load the canvases to prevent lagging on first activation.
    private void PreloadCanvas()
    {
        if (promptCanvas != null)
        {
            promptCanvas.SetActive(true);
            promptCanvas.GetComponentInChildren<TMP_Text>().ForceMeshUpdate();
            promptCanvas.SetActive(false);
        }
    }

    public void PauseGameWithoutMenu()
    {
        PauseGame();

        if (optionsCanvas != null)
        {
            optionsCanvas.SetActive(false);
            pauseMenu.SetActive(false);
        }
    }

    public void PauseGame()
    {
        GamePaused = !GamePaused;

        if (GamePaused)
        {
            Time.timeScale = 0;
            AudioListener.pause = true;

            if (optionsCanvas != null)
            {
                optionsCanvas.SetActive(true);
                pauseMenu.SetActive(true);
            }

            if (promptCanvas != null)
                promptCanvas.SetActive(false);

            if (conversationCanvas != null)
                conversationCanvas.SetActive(false);

            if (lockCursorWhilePlaying)
                UnlockCursor();
        }
        else
        {
            Time.timeScale = 1;
            AudioListener.pause = false;

            if (optionsCanvas != null)
            {
                optionsCanvas.SetActive(false);
                pauseMenu.SetActive(false);
            }

            if (optionsMenu != null)
            {
                optionsMenu.DeactivateOptions();
                optionsMenu.OptionsCancelled();
            }

            if (conversationCanvas != null)
                conversationCanvas.SetActive(true);

            if (lockCursorWhilePlaying)
            {
                if (dialoguePanel == null || !dialoguePanel.activeInHierarchy)
                    LockCursor();
            }
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;

        GamePaused = false;

        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    public void LoadLevel(int buildIndex)
    {
        Time.timeScale = 1;
        AudioListener.pause = false;

        GamePaused = false;

        SceneManager.LoadScene(buildIndex);
    }

    public void ActivatePauseMenu()
    {
        pauseMenu.SetActive(true);
    }

    public void DeactivatePauseMenu()
    {
        pauseMenu.SetActive(false);
    }

    public void RespawnPlayer()
    {
        if (PlayerRespawn.instance != null)
            PlayerRespawn.instance.Respawn();

        PauseGame();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
