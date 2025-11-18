using DialogueEditor;
using System;
using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable
{
    public bool isCollected = false;
    public event Action OnCollected;

    [SerializeField] private GameObject promptUI;

    public void interact()
    {
        CollectItem();
    }

    public void showPrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(true);
    }

    public void hidePrompt() 
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    public void exitInteraction() { }

    public void CollectItem()
    {
        if (OnCollected != null)
            OnCollected.Invoke();

        isCollected = true;
        gameObject.SetActive(false);
    }
}
