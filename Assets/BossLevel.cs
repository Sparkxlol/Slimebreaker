using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/BossDialogue")]
public class BossDialogue : ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string text;
    public AudioClip voiceline;
}

[System.Serializable]
public class BossCollectible
{
    public Collectible collectible;
    public List<QuestTarget> targets;
}

public class BossLevel : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private List<BossCollectible> collectibles;
    [SerializeField] private GameObject boss;
    private BossUI bossUI;

    [Header("Lines")]
    [SerializeField] private BossDialogue bossDialogue;
    private int dialogueIndex = 0;

    private List<Action> enabledEvents;

    private void Awake()
    {
        if (boss == null)
        {
            Debug.LogError("Boss not assigned in BossLevel");
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        bossUI = GameObject.FindGameObjectWithTag("LevelCanvas").GetComponent<LevelCanvas>().bossCanvas.GetComponent<BossUI>();

        if (bossUI == null)
            Debug.LogError("BossUI does not exist or cannot be found by LevelCanvas");
    }

    private void OnEnable()
    {
        foreach (BossCollectible collectible in collectibles)
        {
            Action enabledEvent = () => TargetCollected(collectible);

            enabledEvents.Add(enabledEvent);
            collectible.collectible.OnCollected += enabledEvent;
            
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < collectibles.Count; i++)
        {
            collectibles[i].collectible.OnCollected -= enabledEvents[i];
        }

        enabledEvents.Clear();
    }

    private void TargetCollected(BossCollectible bossCollectible)
    {
        AudioManager.instance.PlayVoiceline(bossDialogue.lines[dialogueIndex].voiceline);
        bossUI.SetBossLine(bossDialogue.lines[dialogueIndex].text);

        foreach (QuestTarget target in bossCollectible.targets)
        {
            target.QuestCompleted();
        }

        dialogueIndex++;
    }
}
