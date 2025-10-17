using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Rigidbody rb;

    public Transform startingPoint;
    private Transform respawnPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        respawnPoint = startingPoint;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Hazard"))
        {
            Respawn();
        }

        if(other.CompareTag("CheckPoint"))
        {
            SetCheckPoint(other.transform);
        }
    }

    private void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = respawnPoint.position;
    }

    private void SetCheckPoint(Transform loc)
    {
        respawnPoint = loc;
    }
    
}
