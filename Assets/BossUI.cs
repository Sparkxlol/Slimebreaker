using TMPro;
using UnityEngine;

public class BossUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI text;

    public void SetBossLine(string line)
    {
        text.text = line;
    }
}
