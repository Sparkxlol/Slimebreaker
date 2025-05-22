using NUnit.Framework.Internal;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody rb;
    public Transform cameraTransform;

    //movement settings
    public float groundAcceleration = 10f;
    public float airAcceleration = 10f;
    public float maxInputSpeed = 20f;

    //jump settings
    public float minJumpForce = 4f;
    public float maxJumpForce = 12;
    public float maxJumpChargeTime = 1f;

    //bounce settings
    public float defaultBounceMult = 0.5f;
    public float releaseBufferTime = 0.15f;
    public float minimumBounceVelocity = .1f;


    //surface checking
    //should be half player height
    public float surfaceCheckRadius = 0.2f;
    public LayerMask surfaceLayer;

    private Vector3 inputDirection;
    private Vector3 currentSurfaceNormal = Vector3.up;
    bool onSurface;
    float lastTouchTime;

    bool jumpHeld;
    float jumpChargeTime;
    bool bounceReady;
    Vector3 bounceReadyNormal;

    bool justBounced;

    bool releaseBufered;
    float releaseTimer;
    float bufferedChargePercent;

    Vector3 preImpactVelocity;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;
    }

    void Update()
    {
        HandleMovementInput();
        HandleJumpInput();
    }

    void HandleMovementInput()
    {
        //Camera relative input direction
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        //combine into directio vector
        Vector3 desiredDirection = camForward * vertical + camRight * horizontal;
        inputDirection = desiredDirection.normalized;


    }

    void HandleJumpInput()
    {
        //need to check first if space is released (keyup)
        //if so, check if on surface or last time touching surface was within buffer time
        //run bounce function using a multiplier depending on how long space was held
        if(Input.GetKeyUp(KeyCode.Space))
        {
            float chargePercent = Mathf.Clamp01(jumpChargeTime / maxJumpChargeTime);

            Debug.Log("IN HANDLE JUMP");

            if(onSurface || bounceReady)
            {
                Debug.Log("on surface is true");
                //if(bounceReady)
                //{
                //    Debug.Log("Bounce");
                //    //this will be a second bounce on top of default bounce
                //    Bounce(preImpactVelocity, currentSurfaceNormal, defaultBounceMult * chargePercent, false);
                //}
                //else
                //{
                //    Debug.Log("REgular jump");
                //    //this will be regular jump

                //}
                Bounce(rb.linearVelocity + preImpactVelocity, currentSurfaceNormal, defaultBounceMult * chargePercent);
                bounceReady = false;


            }

            jumpChargeTime = 0f;
            jumpHeld = false;
            return;
        }



        //if space was pressed (keydown) && jumpCharge timer not already started
        //start charge timer
        if (Input.GetKeyDown(KeyCode.Space) && !jumpHeld)
        {
            jumpHeld = true;
            jumpChargeTime = 0f;

        }

        if(jumpHeld)
        {
            jumpChargeTime += Time.deltaTime;
            jumpChargeTime = Mathf.Min(jumpChargeTime, maxJumpChargeTime);
        }



    }

    //decreases buffer timer so that player cannot use big bounce midair
    void UpdateReleaseBufferTimer()
    {
        if((Time.time - lastTouchTime) > releaseBufferTime)
        {
            bounceReady = false;
        }
    }


    void FixedUpdate()
    {
        onSurface = CheckSurface(out currentSurfaceNormal);
        

        float accel = onSurface ? groundAcceleration : airAcceleration;

        Vector3 moveOnSurface = Vector3.ProjectOnPlane(inputDirection, currentSurfaceNormal).normalized;

        //STOP acceleration for a frame after bounce
        if(!justBounced)
        {
            Accelerate(moveOnSurface, accel, maxInputSpeed);
        }
        justBounced = false;

        
        
        UpdateReleaseBufferTimer();
        AlignWithSurface();
        
    }

    void Accelerate(Vector3 direction, float acceleration, float maxMoveSpeed)
    {
        //dont move if no input or canceling input
        if (direction.sqrMagnitude == 0f) return;

        Vector3 velocity = rb.linearVelocity;

        float currentSpeedInDir = Vector3.Dot(velocity, direction);

        //only accelerate if under max speed in input direction
        if(currentSpeedInDir < maxMoveSpeed)
        {
            //make sure player doesnt accelerate over max move speed
            float speedDiff = maxMoveSpeed - currentSpeedInDir;
            float accelAmount = Mathf.Min(acceleration * Time.fixedDeltaTime, speedDiff);

            rb.AddForce(direction * accelAmount, ForceMode.VelocityChange);

        }


    }

   
    
    bool CheckSurface(out Vector3 surfaceNormal)
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f; //Slight offset to avoid embedded colliders
        float checkDistance = 1f; //Adjust based on player height

        //detect surface using spherecast downward (relative to player's up)
        if (Physics.SphereCast(origin, surfaceCheckRadius, -transform.up, out hit, checkDistance, surfaceLayer))
        {
            surfaceNormal = hit.normal;
            return true;
        }

        surfaceNormal = Vector3.up;
        return false;
    }

    void AlignWithSurface()
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, currentSurfaceNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 2f);
    }

    //bouncing
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision");
        if (((1 << collision.gameObject.layer) & surfaceLayer) == 0) return;
        
        ContactPoint contact = collision.GetContact(0);
        Vector3 normal = contact.normal;

        preImpactVelocity = collision.relativeVelocity;
        

        if (jumpHeld) //jump is still being held, the player still has the buffer time to release
        {
            bounceReady = true;

        }
        
        Bounce(preImpactVelocity, normal, defaultBounceMult);
        lastTouchTime = Time.time;
    }

    
    void Bounce(Vector3 impactVelocity, Vector3 normal, float mult) 
    {
        //jump
        
        Debug.Log("regu jump");
        float jumpImpulse = Mathf.Lerp(minJumpForce, maxJumpForce, Mathf.Clamp01(mult));

        Vector3 vel = rb.linearVelocity;
        float vn = Vector3.Dot(vel, normal);
        if (vn < 0f)
        {
            vel -= vn * normal;
        }

        rb.linearVelocity = vel;
        rb.AddForce(normal * jumpImpulse, ForceMode.VelocityChange);
        
        
    }


}
