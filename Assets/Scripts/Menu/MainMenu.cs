using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject optionsCanvas;
    [SerializeField] private GameObject creditsCanvas;
    [SerializeField] private int scene = 1; // Must be changed to be accurate to save.

    private void Start()
    {
        menuCanvas.SetActive(true);
        optionsCanvas.SetActive(false);
    }

    public void PlayClicked()
    {
        SceneManager.LoadScene(scene);
    }

    public void OptionsClicked()
    {
        menuCanvas.SetActive(false);
        optionsCanvas.SetActive(true);
    }

    public void OptionsClosed()
    {
        menuCanvas.SetActive(true);
        optionsCanvas.SetActive(false);
    }

    public void CreditsClicked()
    {
        menuCanvas.SetActive(false);
        creditsCanvas.SetActive(true);
    }

    public void CreditsClosed()
    {
        menuCanvas.SetActive(true);
        creditsCanvas.SetActive(false);
    }
}
