using UnityEngine;
using Unity.Cinemachine;

public class CameraLock : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CinemachineInputAxisController inputController;

    

    public void LockCameraInput()
    {
        inputController.enabled = false;
    }
    public void UnlockCameraInput()
    {
        inputController.enabled = true;
    }
}
