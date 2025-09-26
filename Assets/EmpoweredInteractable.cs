using UnityEngine;

public class EmpoweredInteractable : MonoBehaviour
{

    private bool pickedUp = false;

   
    public void OnTriggerEnter(Collider other)
    {
        if (pickedUp) return;


        if(other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();

            if (pm != null) 
            {
                if (!pm.empoweredReady)
                {
                    pm.empoweredReady = true;
                    pickedUp = true;
                }
            }

        }
    }

    


}
