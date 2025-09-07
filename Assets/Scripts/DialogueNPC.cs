using UnityEngine;
using DialogueEditor;

public class DialogueNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private NPCConversation conversation;
    [SerializeField] private GameObject promptUI;

    public void interact()
    {
        ConversationManager.Instance.StartConversation(conversation);
    }

    public void showPrompt()
    {
        promptUI.SetActive(true);
    }

    public void hidePrompt() 
    {
        promptUI.SetActive(false);
    }

    public void exitInteraction()
    {
        ConversationManager.Instance.EndConversation();
    }
}

