using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueLine
{
    public string text;
    public AudioClip voiceline;
}

[System.Serializable]
public class BossTarget
{
    public GameObject NPC;
    public List<QuestTarget> targets;
    public bool used = false;
}

public class BossLevel : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private List<Collectible> collectibles;
    private bool collectibleActive = false;
    [SerializeField] private List<BossTarget> bossTargets;
    [SerializeField] private List<QuestTarget> bossDeathTargets;
    private BossUI bossUI;

    [Header("Lines")]
    [SerializeField] private BossDialogue bossDialogue;
    [SerializeField] private DialogueLine bossDeathDialogue;
    private int dialogueIndex = 0;

    private int targetsUsed = 0;

    [Header("Timer")]
    [SerializeField] private float maxTime = 180f;
    public float currentTime = 0;


    private void Start()
    {
        bossUI = GameObject.FindGameObjectWithTag("LevelCanvas").GetComponent<LevelCanvas>().bossCanvas.GetComponent<BossUI>();

        if (bossUI == null)
            Debug.LogError("BossUI does not exist or cannot be found by LevelCanvas");

        currentTime = maxTime;
    }

    private void Update()
    {
        currentTime -= Time.deltaTime;
        bossUI.SetBossTime(currentTime);

        if (currentTime <= 1f)
        {
            PlayerRespawn.instance.RemoveActiveCheckpoint();
            GameManager.instance.LoadLevel(4);
        }
    }

    private void OnEnable()
    {
        foreach (Collectible collectible in collectibles)
        {
            collectible.OnCollected += TargetCollected;
        }

        PlayerRespawn.OnDeath += PlayerDied;
    }

    private void OnDisable()
    {
        foreach (Collectible collectible in collectibles)
        {
            collectible.OnCollected -= TargetCollected;
        }

        PlayerRespawn.OnDeath -= PlayerDied;
    }

    private void PlayerDied()
    {
        currentTime -= 30f;
    }

    private void TargetCollected()
    {
        collectibleActive = true;

        foreach (Collectible collectible in collectibles)
        {
            collectible.canCollect = false;
        }
    }

    public void OnNPCEvent(GameObject NPC)
    {
        if (!collectibleActive) return;

        BossTarget usedTarget = null;
        foreach (BossTarget target in bossTargets)
        {
            if (target.NPC == NPC) usedTarget = target;
        }

        if (usedTarget == null || usedTarget.used) return;

        usedTarget.used = true;
        targetsUsed++;
        collectibleActive = false;

        foreach (Collectible collectible in collectibles)
        {
            collectible.canCollect = true;
        }

        foreach (QuestTarget target in usedTarget.targets)
        {
            target.QuestCompleted();
        }

        if (targetsUsed == bossTargets.Count)
        {
            StartCoroutine(StartBossDeath());
            return;
        }

        StartCoroutine(StartBossVoiceline());
    }

    private IEnumerator StartBossVoiceline()
    {
        yield return new WaitForSeconds(1.5f);

        AudioManager.instance.PlayVoiceline(bossDialogue.lines[dialogueIndex].voiceline);
        bossUI.SetBossLine(bossDialogue.lines[dialogueIndex].text, bossDialogue.lines[dialogueIndex].voiceline.length);

        dialogueIndex++;
    }

    private IEnumerator StartBossDeath()
    {
        yield return new WaitForSeconds(7.5f);

        AudioManager.instance.PlayVoiceline(bossDeathDialogue.voiceline);
        bossUI.SetBossLine(bossDeathDialogue.text, bossDeathDialogue.voiceline.length);

        foreach (QuestTarget target in bossDeathTargets)
        {
            target.QuestCompleted();
        }

        yield return new WaitForSeconds(4f);

        GameManager.instance.LoadMainMenu();
    }
}
