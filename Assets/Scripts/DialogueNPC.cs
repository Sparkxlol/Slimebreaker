using UnityEngine;
using DialogueEditor;
using System.Collections.Generic;
using TMPro;

public class DialogueNPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    public CameraLock CameraLock;
    [SerializeField] private NPCConversation conversation;
    [SerializeField] private GameObject promptUI;
    [SerializeField] public bool canInteract = true;

    [Header("Animation")]
    [SerializeField] private Animator anim;

    private HashSet<string> animParams = new HashSet<string>();

    private void Start()
    {
        foreach (var param in anim.parameters)
        {
            animParams.Add(param.name);
        }
    }

    public void interact()
    {
        if (!canInteract) return;

        GameManager.instance.UnlockCursor();

        ConversationManager.Instance.StartConversation(conversation);
        CameraLock.LockCameraInput();

        if (animParams.Contains("speaking"))
            anim.SetBool("speaking", true);
    }

    public void showPrompt()
    {
        if (!canInteract) return;

        promptUI.SetActive(true);
    }

    public void hidePrompt() 
    {
        if (!canInteract) return;

        promptUI.SetActive(false);
    }

    public void exitInteraction()
    {
        if (!canInteract) return;

        GameManager.instance.LockCursor();

        ConversationManager.Instance.EndConversation();
        CameraLock.UnlockCameraInput();

        if (animParams.Contains("speaking"))
            anim.SetBool("speaking", false);
    }

    public void SwitchConversation(NPCConversation newConversation)
    {
        hidePrompt();
        exitInteraction();

        conversation = newConversation;
    }
}

