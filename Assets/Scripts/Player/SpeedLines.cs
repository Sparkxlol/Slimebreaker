using UnityEngine;

// Not done - don't flame

public class SpeedLines : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject lineEffectPrefab;

    [Header("Range")]
    [Tooltip("X/Y = Up/Down offset from center.")]
    [SerializeField] private float offsetMaxDistance = 1.0f;
    [SerializeField] private float behindDistance = 2f;

    [Header("Speed")]
    [SerializeField] private float minSpeed = 50f;
    [SerializeField] private float spawnSpeed = 1.0f;
    private float spawnTime = 0;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        spawnTime += Time.deltaTime;

        if (rb.linearVelocity.magnitude > minSpeed && spawnTime >= spawnSpeed)
        {
            CreateLine();

            spawnTime = 0;
        }
    }

    private void CreateLine()
    {
        GameObject line = Instantiate(lineEffectPrefab, this.transform);
        SpeedLine speedLine = line.GetComponent<SpeedLine>();

        if (!speedLine)
        {
            Destroy(line.gameObject);
            return;
        }

        speedLine.SetValues(Random.Range(-offsetMaxDistance, offsetMaxDistance), Random.onUnitSphere, behindDistance);
        speedLine.SetPlayer(rb, this.transform);
    }
}
