using Unity.Cinemachine;
using UnityEditor.U2D;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    
    public CinemachineCamera thirdPersonCam;
    public CinemachineCamera firstPersonCam;

    
    private PlayerMovement playerMovement;


    [Header("Player Visibility")]
    [SerializeField] private GameObject playerModel;
    

    [Header("Camera FOV Settings")]
    [SerializeField] private float maxFOVSpeed = 100f;
    
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float defaultFOV = 90f;
    [SerializeField] private float minFOV = 40f;
    [SerializeField] private float maxFOV = 140f;
    [SerializeField] private float chargeZoomSpeed = 1f;




    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(playerMovement.chargeTime == playerMovement.maxChargeTime)
        {
            firstPersonCam.Priority = 10;
            thirdPersonCam.Priority = 5;
            SetPlayerVisible(false);
            return;
        }
        else
        {
            thirdPersonCam.Priority = 10;
            firstPersonCam.Priority = 5;
            SetPlayerVisible(true);
        }

        float velocity = playerMovement.rb.linearVelocity.magnitude;

        if(playerMovement.chargeTime > 0f)
        {
            
            
            thirdPersonCam.Lens.FieldOfView = Mathf.Lerp(thirdPersonCam.Lens.FieldOfView, minFOV, Time.deltaTime * chargeZoomSpeed);
        }
        else
        {
            float speedFOV = Mathf.Clamp01(velocity / maxFOVSpeed);
            float goalFOV = Mathf.Lerp(defaultFOV, maxFOV, speedFOV);
            thirdPersonCam.Lens.FieldOfView = Mathf.Lerp(thirdPersonCam.Lens.FieldOfView, goalFOV, Time.deltaTime * zoomSpeed);

            
        }


    }



    void SetPlayerVisible(bool visible)
    {
        Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }
    }



}
