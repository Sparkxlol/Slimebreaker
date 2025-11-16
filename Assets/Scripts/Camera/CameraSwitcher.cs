using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    
    public CinemachineCamera thirdPersonCam;
    public CinemachineCamera firstPersonCam;

    public CinemachinePanTilt firstPersonPanTilt;
    public CinemachineOrbitalFollow thirdPersonOrbitalFollow;

    private PlayerMovement playerMovement;


    [Header("Player Visibility")]
    [SerializeField] private GameObject playerModel;
    

    [Header("Camera FOV Settings")]
    [SerializeField] private float maxFOVSpeed = 100f;
    
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float defaultFOV = 90f;
    [SerializeField] private float minFOV = 20f;
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
            AlignCamera(firstPersonCam, thirdPersonCam);
            firstPersonCam.Priority = 10;
            thirdPersonCam.Priority = 5;
            SetPlayerVisible(false);
            return;
        }
        else
        {
            AlignCamera(thirdPersonCam, firstPersonCam);
            SyncFirstPersonTiltToFreeLook();
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

    void SyncFirstPersonTiltToFreeLook()
    {
        //this goes between 45 Looking down to -10, looking up
        float thirdPersonAngle = thirdPersonOrbitalFollow.VerticalAxis.Value;
        if(thirdPersonAngle > 0f)
        {
            float newFirstPersonAngle = thirdPersonAngle * (70f / 45f);
        }

        // Clamp to your FP camera’s tilt limits
        if (firstPersonPanTilt != null)
        {
            firstPersonPanTilt.TiltAxis.Value = thirdPersonAngle;
        }
    }

 
    void AlignCamera(CinemachineCamera targetCam, CinemachineCamera sourceCam)
    {
        if (targetCam == null || sourceCam == null)
            return;

        // Make target camera look in the same direction and come from the same point
        targetCam.transform.SetPositionAndRotation(
            sourceCam.transform.position,
            sourceCam.transform.rotation
        );
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
