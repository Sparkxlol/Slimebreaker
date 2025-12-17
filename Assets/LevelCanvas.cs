using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelCanvas : MonoBehaviour
{
    [Header("Pausing")]
    public GameObject optionsCanvas;
    public GameObject pauseMenu;
    public OptionsMenu optionsMenu;

    [Header("Canvas")]
    public GameObject promptCanvas;
    public GameObject conversationCanvas;
    public GameObject dialoguePanel;
    public GameObject bossCanvas;

    [Header("Buttons")]
    public EventTrigger exitTopperEventTrigger;
    public Button saveOptionsButton;
    public Button menuButton;
    public Button settingsButton;
    public Button unstuckButton;
    public Button exitButton;

    private void Awake()
    {
        BossUI bossUI = GetComponentInChildren<BossUI>();

        bossCanvas =  (bossUI) ? bossUI.gameObject : null;
    }
}
