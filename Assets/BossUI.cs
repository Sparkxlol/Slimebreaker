using TMPro;
using UnityEngine;

public class BossUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject background;

    private float maxDuration = 1f;
    private float currentDuration = 0;

    private void Start()
    {
        background.SetActive(false);
    }

    private void Update()
    {
        currentDuration += Time.deltaTime;

        if (currentDuration > maxDuration && background.activeInHierarchy)
        {
            background.SetActive(false);
        }
    }

    public void SetBossLine(string line, float duration)
    {
        text.text = line;
        maxDuration = duration;

        background.SetActive(true);
    }
}
