using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Transform player;
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(0, 3, -6); //behind and above player
    public float rotationSpeed = 5f;

    private float yaw = 0f;
    private float pitch = 0f;

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position;


        //loooking with mouse
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        //left or right
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -30f, 60f);

        //rotate pivot
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        //move camera
        cameraTransform.position = transform.position + transform.rotation * cameraOffset;
        cameraTransform.LookAt(player.position + Vector3.up * 1.5f);


    }
}
