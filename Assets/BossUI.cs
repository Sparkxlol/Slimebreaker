using TMPro;
using UnityEngine;

public class BossUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject background;
    [SerializeField] private TextMeshProUGUI timeText;

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
        currentDuration = 0;

        background.SetActive(true);
    }

    public void SetBossTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timeText.text = minutes + ":" + seconds.ToString("00");
    }
}
