using System;
using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable
{
    public bool isCollected = false;
    public event Action OnCollected;
    public bool canCollect = true;

    [SerializeField] private GameObject promptUI;

    [Header("Pickup")]
    [SerializeField] private bool pickupOnTrigger = false;

    [Header("Teleport")]
    [SerializeField] private bool teleportPlayer = false;
    [SerializeField] private Transform teleportLocation;

    public void interact()
    {
        if (!canCollect) return;

        CollectItem();
    }

    public void showPrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(true);

        if (pickupOnTrigger)
            interact();
    }

    public void hidePrompt() 
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    public void exitInteraction() { }

    public void CollectItem()
    {
        isCollected = true;

        if (OnCollected != null)
            OnCollected.Invoke();

        if (teleportPlayer && teleportLocation != null && PlayerRespawn.instance != null)
            PlayerRespawn.instance.Teleport(teleportLocation);

        if (promptUI != null)
            promptUI.SetActive(false);

        gameObject.SetActive(false);
    }
}
