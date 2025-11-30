using DialogueEditor;
using System;
using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable
{
    public bool isCollected = false;
    public event Action OnCollected;

    [SerializeField] private GameObject promptUI;

    [Header("Teleport")]
    [SerializeField] private bool teleportPlayer = false;
    [SerializeField] private Transform teleportLocation;

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

        if (teleportPlayer && teleportLocation != null && PlayerRespawn.instance != null)
            PlayerRespawn.instance.Teleport(teleportLocation);

        isCollected = true;
        gameObject.SetActive(false);
    }
}
