using UnityEngine;

public class DestroyAfterCreation : MonoBehaviour
{
    public float DestructionTime { get; set; } = 1.0f;
    private float currentDestructionTime = 0.0f;

    void Update()
    {
        currentDestructionTime += Time.deltaTime;

        if (currentDestructionTime >= DestructionTime)
            Destroy(this.gameObject);
    }
}
