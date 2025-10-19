using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;
    static public bool GamePaused { get; private set; }

    [Header("Pausing")]
    [SerializeField] private bool gameplayScene;
    [SerializeField] private GameObject optionsCanvas;
    [SerializeField] private OptionsMenu optionsMenu;

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
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && gameplayScene)
        {
            PauseGame();
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
                optionsCanvas.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            AudioListener.pause = false;

            if (optionsCanvas != null)
                optionsCanvas.SetActive(false);

            if (optionsMenu != null)
                optionsMenu.OptionsCancelled();
        }
    }
}
