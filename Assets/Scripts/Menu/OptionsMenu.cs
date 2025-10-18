using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    // Topper order should be decided based on location in UI from left-right.
    [Header("Toppers")]
    [SerializeField] private GameObject activeTopperHeader;
    [SerializeField] private GameObject inactiveTopperHeader;
    [SerializeField] private List<GameObject> toppers;
    [SerializeField] private int activeTopper;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    [Header("Menus")]
    [SerializeField] private List<GameObject> menus;

    [Header("Video Options")]
    [SerializeField] private TextMeshProUGUI fullscreenValue;
    [SerializeField] private TextMeshProUGUI windowedValue;

    void Start()
    {
        ResetActiveToppers();
        LoadOptions();
    }

    private void LoadOptions()
    {
        // Video Settings
        fullscreenValue.text = (OptionsManager.instance.Fullscreen) ? "On" : "Off";
        windowedValue.text = (OptionsManager.instance.WindowedFullscreen) ? "On" : "Off";
    }

    void ResetActiveToppers()
    {
        foreach (var topper in toppers)
        {
            topper.transform.SetParent(inactiveTopperHeader.transform);
            topper.GetComponent<Image>().color = inactiveColor;
        }

        toppers[activeTopper].transform.SetParent(activeTopperHeader.transform);
        toppers[activeTopper].GetComponent<Image>().color = activeColor;

        for (int i = 0; i < menus.Count; i++)
        {
            menus[i].SetActive(false);
        }
        menus[activeTopper].SetActive(true);
    }

    public void ChangeActiveTopper(int indexOfTopper)
    {
        toppers[activeTopper].transform.SetParent(inactiveTopperHeader.transform);
        toppers[activeTopper].GetComponent<Image>().color = inactiveColor;

        toppers[indexOfTopper].transform.SetParent(activeTopperHeader.transform);
        toppers[indexOfTopper].GetComponent<Image>().color = activeColor;

        menus[activeTopper].SetActive(false);
        menus[indexOfTopper].SetActive(true);

        activeTopper = indexOfTopper;
    }

    public void FullscreenClicked()
    {
        OptionsManager.instance.Fullscreen = !OptionsManager.instance.Fullscreen;
        fullscreenValue.text = (OptionsManager.instance.Fullscreen) ? "On" : "Off";
    }

    public void WindowedClicked()
    {
        OptionsManager.instance.WindowedFullscreen = !OptionsManager.instance.WindowedFullscreen;
        windowedValue.text = (OptionsManager.instance.WindowedFullscreen) ? "On" : "Off";
    }
}
