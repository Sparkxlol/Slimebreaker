using UnityEngine;

public interface IInteractable
{
    void interact();
    void showPrompt();
    void hidePrompt();
    void exitInteraction();
}
