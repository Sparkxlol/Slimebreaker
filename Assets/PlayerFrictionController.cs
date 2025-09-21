using UnityEngine;

public class PlayerFrictionController : MonoBehaviour
{
    public PhysicsMaterial normalFriction;
    public PhysicsMaterial slideFriction;

    private Collider col;
    void Start()
    {
        col =  GetComponent<Collider>();
        col.material = normalFriction;
    }

    public void startSlide()
    {
        col.material = slideFriction;
    }

    public void stopSlide()
    {
        col.material = normalFriction;
    }
}
