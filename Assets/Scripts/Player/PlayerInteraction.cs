using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    private IInteractable currentinteractable;

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null)
        {
            currentinteractable = interactable;
            
            currentinteractable.showPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        currentinteractable?.hidePrompt();
        

        if (interactable != null && interactable == currentinteractable)
        {
            currentinteractable.exitInteraction();
            currentinteractable = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentinteractable != null && Input.GetKeyDown(interactKey))
        {
            currentinteractable.hidePrompt();
            currentinteractable.interact();
        }
    }
}
