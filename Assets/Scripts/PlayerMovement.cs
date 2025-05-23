using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody rb;
    public Transform cameraTransform;
    public Transform visualTransform;

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
    public float releaseBufferTime = 0.35f;
    public float minimumBounceVelocity = .1f;

    //stickiness
    public float stickLeft;
    public bool isSticking;


    //surface checking
    //should be half player height
    public float surfaceCheckRadius = 0.2f;
    public LayerMask surfaceLayer;

    private Vector3 inputDirection;
    private Vector3 currentSurfaceNormal = Vector3.up;
    
    float lastTouchTime;

    bool jumpHeld;
    float jumpChargeTime;
    
    Vector3 bounceReadyNormal;

    bool justBounced;

    bool releaseBufered;
    float releaseTimer;
    float bufferedChargePercent;

    Vector3 preImpactVelocity;

    private bool onSurface;

    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log("Exit collision");
        onSurface = false;
        lastTouchTime = Time.time;
    }

    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log("collision stay");
        onSurface = true;
        
        currentSurfaceNormal = collision.GetContact(0).normal;
    }
    private void OnCollisionEnter(Collision collision)
    { 
        //Debug.Log("Collision eneter");
        if (((1 << collision.gameObject.layer) & surfaceLayer) == 0) return;

        ContactPoint contact = collision.GetContact(0);
        Vector3 normal = contact.normal;

        preImpactVelocity = collision.relativeVelocity;



        onSurface = true;
        currentSurfaceNormal = normal;
        
    }

    public bool touchingSurface()
    {
        return onSurface;
    }


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

    //PLayer can hold left click, until they hit a surface
    //once they hit a surface, the stickiness meter will slowly deplete.
    //if they release left click or run out of stickiness, they will unstick from the surface
    void HandleStickInput()
    {
        //hold stick
        if (Input.GetMouseButtonDown(0))
        {
            if (touchingSurface())
            {
                isSticking = true;
                //stick to wall
                //remove gravity
                //deplete velocity towards surface stuck to

            }
        }
    }
    void HandleJumpInput()
    {
        //need to check first if space is released (keyup)
        //if so, check if on surface or last time touching surface was within buffer time
        //run bounce function using a multiplier depending on how long space was held
        if(Input.GetKeyUp(KeyCode.Space))
        {
            float chargePercent = Mathf.Clamp01(jumpChargeTime / maxJumpChargeTime);

            //Debug.Log("IN HANDLE JUMP");
            //Debug.Log("touching surface = " + touchingSurface() + " Time.time- lastTouchTime = " + (Time.time - lastTouchTime) + " < Releasse buffer = " + releaseBufferTime);
            //Debug.Log("Last Touch time" + lastTouchTime);
            //Debug.Log("Time.time" + Time.time);
            if((touchingSurface() || (Time.time - lastTouchTime) <= releaseBufferTime) && !isSticking)
            {
                Debug.Log("on surface is true");
                
                Bounce(rb.linearVelocity + preImpactVelocity, currentSurfaceNormal, defaultBounceMult * chargePercent);
                


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

    
    


    void FixedUpdate()
    {
        bool isOnSurface = touchingSurface();
        //Debug.Log("Cursufacenormal = " + currentSurfaceNormal);
        Debug.DrawRay(transform.position, -transform.up * 1f, Color.red);


        float accel = isOnSurface ? groundAcceleration : airAcceleration;

        Vector3 moveOnSurface = Vector3.ProjectOnPlane(inputDirection, currentSurfaceNormal).normalized;

        //STOP acceleration for a frame after bounce
        if(!justBounced)
        {
            Accelerate(moveOnSurface, accel, maxInputSpeed);
        }
        justBounced = false;

        //align character with movement direction NEEDED
        

        
        
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

   
    
  

    //jump
    
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
