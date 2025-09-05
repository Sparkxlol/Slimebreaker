using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody rb;
    public Transform cameraTransform;
    public Transform visualTransform;

    [Header("Movement Settings")]
    public float groundAcceleration = 25f;
    public float airAcceleration = 10f;
    public float maxInputSpeed = 20f;

    [Header("Jump Settings")]
    public float minJumpForce = 4f;
    public float maxJumpForce = 20f;
    public float maxJumpChargeTime = 2f;
    public float lastGrounded;
    public float jumpBufferTime = 1f;
    public float lastJumped;

    [Header("Bounce Settings")]
    public float defaultBounceMult = 0.5f;
    public float releaseBufferTime = 0.35f;
    public float minimumBounceVelocity = .1f;

    [Header("Stickiness")]
    public float stickLeft;
    public bool isSticking;
    public float maxStickCharge = 4f;
    public float stickDepletionRate = 1f;
    public float stickRechargeRate = 1.5f;

    [Header("Glide")]
    public float glideLeft;
    public bool isGliding;
    public float maxGlideCharge = 5f;
    public float glideRechargeRate = 1.5f;
    public float glideDepletionRate = 1f;
    public float glideGravity = 0.3f;


    [Header("Collision Checks")]
    private float groundDistanceCheck = .25f;
    private bool onGround = false;
    private float wallRadiusCheck = 1.0f;
    private bool onWall = false;
    private RaycastHit? wallHit = null;
    private Vector3 wallHitDirection = Vector3.zero;

    //surface checking
    //should be half player height
    public float surfaceCheckRadius = 0.2f;
    public LayerMask surfaceLayer;

    private Vector3 inputDirection;
    private Vector3 currentSurfaceNormal = Vector3.up;
    
    bool jumpHeld;
    float jumpChargeTime;
    
    bool justBounced;

    Vector3 preImpactVelocity;

    void Update()
    {
        HandleMovementInput();
        CheckGrounded();
        CheckWalled();

        HandleJumpInput();
    }

    void FixedUpdate()
    {
        bool isOnSurface = onGround;

        float accel = isOnSurface ? groundAcceleration : airAcceleration;

        Vector3 moveOnSurface = Vector3.ProjectOnPlane(inputDirection, currentSurfaceNormal).normalized;

        //STOP acceleration for a frame after bounce
        if (!justBounced && !onWall)
        {
            Accelerate(moveOnSurface, accel, maxInputSpeed);
        }

        justBounced = false;

        HandleWallRun();
        HandleGlide();
    }

    private void CheckGrounded()
    {
        // Casts a downward raycast and checks if the tag Ground is applied.
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundDistanceCheck))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                Debug.DrawRay(transform.position, Vector3.down * (groundDistanceCheck), Color.red, .1f);

                onGround = true;

                //Start timer to give buffer for jump
                lastGrounded = Time.time;

                return;
            }
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down * (groundDistanceCheck), Color.green, .1f);
        }

        onGround = false;
    }

    private void CheckWalled()
    {
        int mask = ~LayerMask.GetMask("Player");

        // Casts a circle towards the player's direction and checks if touching a sticky surface.
        /*
        if (Physics.Raycast(transform.position, rb.linearVelocity.normalized, out RaycastHit hit, wallRadiusCheck, mask)) //Physics.SphereCast(transform.position + inputDirection * 1.5f, wallRadiusCheck, inputDirection, out RaycastHit hit, wallRadiusCheck + 100))
        {
            if (hit.collider.GetComponent<StickySurface>())
            {
                Debug.DrawRay(transform.position, inputDirection * (wallRadiusCheck), Color.red, .1f);

                wallHit = hit;
                onWall = true;
                return;
            } 
        }
        if (Physics.Raycast(transform.position + new Vector3(0, 2.5f, 0), rb.linearVelocity.normalized, out RaycastHit hit2, wallRadiusCheck, mask)) //Physics.SphereCast(transform.position + inputDirection * 1.5f, wallRadiusCheck, inputDirection, out RaycastHit hit, wallRadiusCheck + 100))
        {
            if (hit2.collider.GetComponent<StickySurface>())
            {
                Debug.DrawRay(transform.position + new Vector3(0, 2.5f, 0), inputDirection * (wallRadiusCheck), Color.red, .1f);

                wallHit = hit2;
                onWall = true;
                return;
            }
        }
        else
        {
            Debug.DrawRay(transform.position, inputDirection * (wallRadiusCheck), Color.green, .1f);
            Debug.DrawRay(transform.position + new Vector3(0, 2.5f, 0), inputDirection * (wallRadiusCheck), Color.green, .1f);
        }
        */

        RaycastHit hit;

        // If previously onWall, cast towards the previousHit's direction.
        if (onWall)
        {
            if (Physics.SphereCast(transform.position + new Vector3(0, wallRadiusCheck, 0), wallRadiusCheck, wallHitDirection, out hit, wallRadiusCheck))
            {
                
                if (hit.collider.GetComponent<StickySurface>())
                {
                    wallHit = hit;
                    onWall = true;

                    return;
                }
                //if not sticky surface, make wallhit and onwall no?
            }
            else
            {
                wallHit = null;
                onWall = false;
            }
        }

        // Check if a wall is nearby towards current velocity's direction.
        if (Physics.SphereCast(transform.position + new Vector3(0, wallRadiusCheck, 0), wallRadiusCheck, rb.linearVelocity.normalized, out hit, wallRadiusCheck))
        {
            if (hit.collider.GetComponent<StickySurface>())
            {
                wallHit = hit;
                onWall = true;

                wallHitDirection = rb.linearVelocity.normalized;
            }
        }
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

        // Rotate player towards camera sight.
        transform.eulerAngles = new Vector3(0, cameraTransform.eulerAngles.y, 0);
    }

    private void HandleGlide()
    {
        if (onGround || onWall)
        {
            isGliding = false;

            if (!isSticking)
            {
                rb.useGravity = true;
            }

            glideLeft += glideRechargeRate * Time.deltaTime;
            glideLeft = Mathf.Min(glideLeft, maxGlideCharge);
            

            return;
        }
        else if (Input.GetMouseButton(1) && glideLeft > 0) 
        {
            //UNFINISHED
            //need to change gravity depending on angle of glide
            isGliding = true;

            glideLeft -= glideDepletionRate * Time.deltaTime;
            glideLeft = Mathf.Max(0, glideLeft);

            rb.useGravity = false;

            //placeholder
            Vector3 vel = rb.linearVelocity;
            vel.y *= glideGravity;
            rb.linearVelocity = vel;

            return;
        }

        //in air
        if (!Input.GetMouseButton(1))
        {
            glideLeft += glideRechargeRate * Time.deltaTime;
            glideLeft = Mathf.Min(glideLeft, maxGlideCharge);
        }

        isGliding = false;
        rb.useGravity = true;

    }
    private void HandleWallRun()
    {
        if (wallHit is RaycastHit hit && Input.GetMouseButton(0) && stickLeft > 0)
        {
            Debug.Log("holding down ");
            rb.useGravity = false; // Turn off gravity
            isSticking = true;

            stickLeft -= stickDepletionRate * Time.deltaTime;
            stickLeft = Mathf.Max(0, stickLeft);

            
            Vector3 wallNormal = hit.normal;
            Vector3 wallUp = Vector3.Cross(Vector3.up, wallNormal).normalized;   // A/D
            Vector3 wallRight = Vector3.Cross(wallUp, wallNormal).normalized;    // W/S


            
            float horizontal = Input.GetAxisRaw("Horizontal"); // A/D = left/right
            float vertical = Input.GetAxisRaw("Vertical");   // W/S = up/down on wall

            // Movement direction on wall plane
            Vector3 moveDir = (wallRight * horizontal + -wallUp * vertical).normalized;

            // Apply velocity on wall
            float wallSpeed = 10f;
            rb.linearVelocity = moveDir * wallSpeed;

        }
        else
        {
            rb.useGravity = true;
            onWall = false;
            wallHit = null;

            if(stickLeft < maxStickCharge && !Input.GetMouseButton(0))
            {
                stickLeft += stickRechargeRate * Time.deltaTime;
                stickLeft = Mathf.Min(maxStickCharge, stickLeft);
            }
        }
    }

    void HandleJumpInput()
    {
        //need to check first if space is released (keyup)
        //if so, check if on surface or last time touching surface was within buffer time
        //run bounce function using a multiplier depending on how long space was held
        if (Input.GetKeyUp(KeyCode.Space))
        {
            float chargePercent = Mathf.Clamp01(jumpChargeTime / maxJumpChargeTime);

            if (onGround || ((Time.time - lastGrounded < jumpBufferTime) && Time.time - lastJumped  > jumpBufferTime))
            {
                lastJumped = Time.time;
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

    void Bounce(Vector3 impactVelocity, Vector3 normal, float mult) 
    {
        float jumpImpulse = Mathf.Lerp(minJumpForce, maxJumpForce, Mathf.Clamp01(mult));

        Debug.Log("Hello");

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
