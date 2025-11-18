using UnityEngine;

public class SpeedLine : MonoBehaviour
{
    private float randomOffset = 1f;
    private Vector3 randomDirection = Vector3.zero;
    private float behindDistance = 1f;

    private Rigidbody playerRb;
    private Transform playerTransform;

    public void SetValues(float randomOffset, Vector3 randomDirection, float behindDistance)
    {
        this.randomOffset = randomOffset;
        this.randomDirection = randomDirection;
        this.behindDistance = behindDistance;
    }

    public void SetPlayer(Rigidbody playerRb, Transform playerTransform)
    {
        this.playerRb = playerRb;
        this.playerTransform = playerTransform;
    }

    void Update()
    {
        Vector3 behindPlayer = -playerRb.linearVelocity.normalized;
        Vector3 randomPerp = Vector3.ProjectOnPlane(randomDirection, behindPlayer).normalized;

        transform.position = playerTransform.position + behindPlayer * behindDistance + randomPerp * randomOffset;
    }
}
