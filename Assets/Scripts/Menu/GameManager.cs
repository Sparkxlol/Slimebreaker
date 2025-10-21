using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;
    static public bool GamePaused { get; private set; }

    [Header("Pausing")]
    [Tooltip("Allows for the pause menu to be activated, if true")]
    [SerializeField] private bool gameplayScene;
    [SerializeField] private GameObject optionsCanvas;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private int mainMenuBuildIndex = 3;

    [Header("Canvas")]
    [SerializeField] private GameObject promptCanvas;

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

        PreloadCanvas();
    }

    void Start()
    {
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
                optionsMenu.OptionsCancelled();

            if (lockCursorWhilePlaying)
                LockCursor();
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

        SceneManager.LoadScene(mainMenuBuildIndex);
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
