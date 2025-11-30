using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugCanvas : MonoBehaviour
{
    private static DebugCanvas instance;

    [SerializeField] private GameObject background;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
        background.SetActive(false);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (!background.activeInHierarchy)
                {
                    background.SetActive(true);
                    GameManager.instance.PauseGameWithoutMenu();
                }
                else
                {
                    background.SetActive(false);
                    GameManager.instance.PauseGameWithoutMenu();
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (background && background.activeInHierarchy) 
            background.SetActive(false);
    }

    public void LoadScene(int buildIndex)
    {
        GameManager.instance.LoadLevel(buildIndex);
    }

    public void ResetCheckpoint()
    {
        if (PlayerRespawn.instance != null)
            PlayerRespawn.instance.RemoveActiveCheckpoint();
    }
}
