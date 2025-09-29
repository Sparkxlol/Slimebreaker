using UnityEngine;
using DialogueEditor;

public class DialogueNPC : MonoBehaviour, IInteractable
{
    public CameraLock CameraLock;
    [SerializeField] private NPCConversation conversation;
    [SerializeField] private GameObject promptUI;
    [SerializeField] private Animator anim;

    public void interact()
    {
        ConversationManager.Instance.StartConversation(conversation);
        CameraLock.LockCameraInput();

        anim.SetBool("speaking", true);
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
        CameraLock.UnlockCameraInput();

        anim.SetBool("speaking", false);
    }
}

