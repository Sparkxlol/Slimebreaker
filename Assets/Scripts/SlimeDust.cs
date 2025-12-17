using UnityEngine;

public class SlimeDust : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private float distanceFromCenter = .75f;
    [SerializeField] private float initialSpeed = 5f;
    [SerializeField] private float speedToAppear = 20f;
    [SerializeField] private float speedModifier = .15f;

    [Header("Components")]
    [SerializeField] private Rigidbody playerRigidbody;
    private PlayerMovement playerMovement;
    private ParticleSystem particleSys;

    void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        particleSys = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (playerRigidbody.linearVelocity.magnitude < speedToAppear || !playerMovement.onGround)
        {
            particleSys.Stop();
            return;
        }

        if (!particleSys.isPlaying)
            particleSys.Play();

        Vector3 playerVelocity = playerRigidbody.linearVelocity;
        transform.forward = -playerVelocity.normalized;
        transform.position = playerMovement.transform.position + -playerVelocity.normalized * distanceFromCenter;

        ParticleSystem.MainModule main = particleSys.main;
        main.startSpeed = initialSpeed * playerVelocity.magnitude * speedModifier;
    }
}
