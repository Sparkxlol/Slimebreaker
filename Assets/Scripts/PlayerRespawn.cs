using System.Linq;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private static PlayerRespawn instance;
    private Checkpoint[] checkpoints;

    private Rigidbody rb;

    public Transform startingPoint;
    private Transform respawnPoint;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        respawnPoint = startingPoint;

        checkpoints = GameObject.FindGameObjectsWithTag("CheckPoint").Select(checkpoint => checkpoint.GetComponent<Checkpoint>()).ToArray();
        
        Checkpoint loadedCheckpoint = LoadCheckpoint();
        if (loadedCheckpoint != null)
        {
            SetCheckPoint(loadedCheckpoint.transform);
            Invoke("Respawn", 0f);
        }
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
            SaveCheckpoint(other.GetComponent<Checkpoint>());
        }
    }

    private void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = respawnPoint.position;
    }

    private void SaveCheckpoint(Checkpoint checkpoint)
    {
        PlayerPrefs.SetString("ACTIVE_CHECKPOINT_ID", checkpoint.CheckpointId);
        PlayerPrefs.Save();
    }

    private Checkpoint LoadCheckpoint()
    {
        string checkpointId = PlayerPrefs.GetString("ACTIVE_CHECKPOINT_ID", "");
        
        if (string.IsNullOrEmpty(checkpointId)) 
            return null;

        return checkpoints.FirstOrDefault(c => c.CheckpointId == checkpointId);
    }

    private void SetCheckPoint(Transform loc)
    {
        respawnPoint = loc;
    }

    [ContextMenu("Remove Active Checkpoint")]
    public void RemoveActiveCheckpoint()
    {
        PlayerPrefs.DeleteKey("ACTIVE_CHECKPOINT_ID");
        Debug.LogWarning("Active checkpoint cleared!");
    }
}
