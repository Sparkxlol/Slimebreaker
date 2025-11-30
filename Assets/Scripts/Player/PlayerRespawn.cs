using System;
using System.Linq;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public static PlayerRespawn instance;
    private Checkpoint[] checkpoints;

    private Rigidbody rb;

    public PlayerMovement pm;

    public Transform startingPoint;
    private Vector3 respawnPoint;

    public static event Action OnDeath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        checkpoints = GameObject.FindGameObjectsWithTag("CheckPoint").Select(checkpoint => checkpoint.GetComponent<Checkpoint>()).ToArray();
        if(pm == null)
        {
            Debug.Log("PLayermovment null in playerrespawn");
        }

        Checkpoint loadedCheckpoint = LoadCheckpoint();
        if (loadedCheckpoint != null)
        {
            SetCheckPoint(loadedCheckpoint.transform.position);
            Respawn();
        }
        else
        {
            SetCheckPoint(startingPoint.position);
            Respawn();
        }

        pm.slideLeft = pm.maxSlideCharge;
        pm.stickLeft = pm.maxStickCharge;
        pm.glideLeft = pm.maxGlideCharge;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Hazard"))
        {
            Respawn();
        }

        if(other.CompareTag("CheckPoint"))
        {
            SetCheckPoint(other.transform.position);
            SaveCheckpoint(other.GetComponent<Checkpoint>());
        }
    }

    public void Respawn()
    {
        OnDeath?.Invoke();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = respawnPoint;

        pm.isGliding = false;
        rb.useGravity = true;
        pm.slideLeft = pm.maxSlideCharge;
        pm.stickLeft = pm.maxStickCharge;
        pm.glideLeft = pm.maxGlideCharge;
    }

    public void Teleport(Transform location)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = location.position;

        pm.isGliding = false;
        rb.useGravity = true;
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

    private void SetCheckPoint(Vector3 position)
    {
        respawnPoint = position;
    }

    [ContextMenu("Remove Active Checkpoint")]
    public void RemoveActiveCheckpoint()
    {
        PlayerPrefs.DeleteKey("ACTIVE_CHECKPOINT_ID");
        Debug.LogWarning("Active checkpoint cleared!");

        SetCheckPoint(startingPoint.position);
    }
}
