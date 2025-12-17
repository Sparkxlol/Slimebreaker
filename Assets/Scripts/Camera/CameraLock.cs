using UnityEngine;
using Unity.Cinemachine;

public class CameraLock : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CinemachineInputAxisController inputController;
    public CinemachineInputAxisController firstpersonInputController;
    public CinemachinePanTilt firstpersonPanTiltController;

    

    public void LockCameraInput()
    {
        firstpersonPanTiltController.enabled = false;
        firstpersonInputController.enabled = false;
        inputController.enabled = false;
    }
    public void UnlockCameraInput()
    {
        firstpersonPanTiltController.enabled = true;
        firstpersonInputController.enabled = true;
        inputController.enabled = true;
    }
}
