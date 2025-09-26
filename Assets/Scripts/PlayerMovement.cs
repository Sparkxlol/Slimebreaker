
using TMPro;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;



public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody rb;
    public Transform cameraTransform;
    public Transform visualTransform;

    [Header("Movement Settings")]
    public float airGravityMultiplier = 0.8f;
    public float groundAcceleration = 25f;
    public float airAcceleration = 50f;
    public float maxInputSpeed = 20f;
    public float groundSlideSpeedMultiplier = 2f;
    [SerializeField] public float wallFrictionStrength = 5f;
    public float airFrictionStrength = 1.5f;

    [Header("Jump Settings")]
    public float minJumpForce = 4f;
    public float maxJumpForce = 20f;
    public float baseJumpMagnitude = 30f;
    
    public float lastGrounded;
    public float jumpBufferTime = 1f;
    public float lastJumped;
    [SerializeField] private bool chargeHeld = false;
    public float chargeTime = 0f;
    public float maxChargeTime = 2f;

    [Header("Bounce Settings")]
    public float defaultBounceMult = 1f;
    public float releaseBufferTime = 0.35f;
    public float minimumBounceVelocity = .1f;

    [Header("Stickiness")]
    public float stickLeft;
    public bool isSticking;
    public float maxStickCharge = 7f;
    public float stickDepletionRate = 1f;
    public float stickRechargeRate = 1.5f;

    [Header("Glide")]
    public float glideLeft;
    public bool isGliding;
    public float maxGlideCharge = 20f;
    public float glideRechargeRate = 1.5f;
    public float glideDepletionRate = 1f;
    public float glideGravityMultiplier = 12f;
    public float diveGravityMultiplier = 25f;
    [SerializeField] private float currentDiveAngle = 0f;
    [SerializeField] private float glideControlStrength = 0.5f;
    [SerializeField] private float maxNoseDiveAngle = 80f;
    [SerializeField] private float noseDiveSpeed = 60f;
    [SerializeField] private float slowDownGravityMultiplier = 3f;
    [SerializeField] private float lastGlidePress = 0f;
    [SerializeField] public bool glideTogglePressed = false;

    [Header("Slide")]
    public bool slideActive;
    public float slideLeft;
    public float maxSlideCharge = 15f;
    public float slideDepletionRate = 1f;
    public float slideRechargeRate = 1.5f;
    public float slideGravityMultiplier = 0.5f;


    [Header("Collision Checks")]
    private bool isOnSurface;
    private float groundDistanceCheck = .25f;
    private bool onGround = false;
    private float wallRadiusCheck = 1.5f;
    private bool onWall = false;
    private RaycastHit? wallHit = null;

    [Header("Empowered Abilities")]
    [SerializeField] public bool empoweredReady = false;
    [SerializeField] public bool empoweredSlide = false;
    [SerializeField] public bool empoweredJump = false;
    [SerializeField] public bool empoweredStick = false;
    [SerializeField] public bool empoweredGlide = false;
    [SerializeField] private float empoweredSlideMultiplier = 2f;
    [SerializeField] private float empoweredJumpMultiplier = 2f;
    [SerializeField] private float empoweredStickMultiplier = 2f;
    [SerializeField] private float empoweredGlideMultiplier = 2f;
    
    private Vector3 wallHitDirection = Vector3.zero;

    //surface checking
    //should be half player height
    public float surfaceCheckRadius = 0.2f;
    public LayerMask surfaceLayer;

    private Vector3 inputDirection;
    private Vector3 currentSurfaceNormal = Vector3.up;

    private Collider col;
    public PhysicsMaterial normalFriction;
    public PhysicsMaterial slideFriction;
    
    bool justBounced;

    Vector3 preImpactVelocity;

    private void Start()
    {
        col = GetComponentInChildren<Collider>();
        col.material = normalFriction;
    }
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.F))
        {
            glideTogglePressed = true;
            lastGlidePress = Time.time;
        }

        if(empoweredReady)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                empoweredJump = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                empoweredSlide = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                empoweredStick = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                empoweredGlide = true;
            }

        }

        HandleSlide();
        HandleMovementInput();
        CheckGrounded();
        CheckWalled();
        

        HandleJumpInput();
    }

    void FixedUpdate()
    {
        isOnSurface = onGround;
        if (!isOnSurface) 
        {
            isOnSurface = isSticking;
        }


        float accel = isOnSurface ? groundAcceleration : airAcceleration;

        Vector3 moveOnSurface = Vector3.ProjectOnPlane(inputDirection, Vector3.up).normalized;

        //STOP acceleration for a frame after bounce
        


        justBounced = false;

        HandleWallRun();
        HandleGlide(moveOnSurface);

        if (!justBounced)
        {
            Accelerate(moveOnSurface, accel, maxInputSpeed);
        }

        glideTogglePressed = false;
    }

    private void HandleSlide()
    {
        if(Input.GetMouseButton(1))
        {         
            slideLeft -= slideDepletionRate * Time.deltaTime;
            slideLeft = Mathf.Max(0, slideLeft);

            if (slideLeft > 0)
            {
                startSlide();
                slideActive = true;
            }
            else
            {
                stopSlide();
                slideActive= false;
            }

            
        }
        else
        {  
            stopSlide();
            slideActive = false;
            slideLeft += slideRechargeRate * Time.deltaTime;
            slideLeft = Mathf.Min(maxSlideCharge, slideLeft);
        }
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
                currentSurfaceNormal = hit.normal;

                //Start timer to give buffer for jump
                lastGrounded = Time.time;
                preImpactVelocity = rb.linearVelocity;

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


        RaycastHit hit;

        // If previously onWall, cast towards the previousHit's direction.
        if (onWall)
        {
            if (Physics.SphereCast(transform.position + new Vector3(0, wallRadiusCheck, 0), wallRadiusCheck, wallHitDirection, out hit, wallRadiusCheck))
            {

                if (hit.collider.GetComponent<StickySurface>())
                {
                    //Debug.Log("On Wall");
                    lastGrounded = Time.time;
                    currentSurfaceNormal = hit.normal;
                    preImpactVelocity = rb.linearVelocity;

                    wallHit = hit;
                    onWall = true;

                    return;
                }
                //if not sticky surface, make wallhit and onwall no?
            }
            else
            {
                Debug.Log("OFF WALL");
                onGround = false;
                wallHit = null;
                onWall = false;
            }




        }



        // Check if a wall is nearby towards current velocity's direction.
        if (Physics.SphereCast(transform.position + new Vector3(0, wallRadiusCheck, 0), wallRadiusCheck, rb.linearVelocity.normalized, out hit, wallRadiusCheck))
        {
            if (hit.collider.GetComponent<StickySurface>())
            {
                //Debug.Log("On Wall 2");
                currentSurfaceNormal = hit.normal;
                lastGrounded = Time.time;
                preImpactVelocity = rb.linearVelocity;

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

    private void HandleGlide(Vector3 inputDir)
    {
        // Handle input toggle first
        if (glideTogglePressed)
        {
            
            lastGlidePress = Time.time;

            // Toggle glide state
            if (!isGliding && glideLeft > 0 && !onGround && !onWall)
            {
                isGliding = true;
                rb.useGravity = false;
            }
            else
            {
                isGliding = false;
                rb.useGravity = true;
            }
        }

        // Handle recharge when not gliding
        if (!isGliding || onGround || onWall)
        {
            glideLeft += glideRechargeRate * Time.deltaTime;
            glideLeft = Mathf.Min(glideLeft, maxGlideCharge);
            currentDiveAngle = 0;

            rb.useGravity = true;
            isGliding = false;
            return;
        }

        // If gliding
        if (isGliding && glideLeft > 0)
        {
            if (rb.linearVelocity.y > 0)
            {
                glideLeft -= glideDepletionRate * Time.deltaTime;
                rb.AddForce(Physics.gravity * slowDownGravityMultiplier, ForceMode.Acceleration);

                if (rb.linearVelocity.y < 0)
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            }
            else
            {
                glideInput(inputDir);
            }
        }
        else
        {
            // Out of glide juice
            isGliding = false;
            rb.useGravity = true;
        }
    }

    private void glideInput(Vector3 inputDir)
    {
        glideLeft -= glideDepletionRate * Time.deltaTime;
        glideLeft = Mathf.Max(0, glideLeft);

        rb.useGravity = false;


        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);
        float speed = rb.linearVelocity.magnitude;

       
        if (inputDir.sqrMagnitude > 0f)
        {
            float maxTurnThisFrame = glideControlStrength * Time.deltaTime;
            Vector3 newHorizontalDir = Vector3.RotateTowards(horizontalVelocity.normalized, inputDir.normalized, maxTurnThisFrame, 0f);
            horizontalVelocity = newHorizontalDir * horizontalVelocity.magnitude;
        }


        //dive 
        bool pressingW = Input.GetKey(KeyCode.W);

        float targetDive = pressingW ? maxNoseDiveAngle : 0f;
        // Smoothly adjust current dive angle toward target
        currentDiveAngle = Mathf.MoveTowards(currentDiveAngle, targetDive, noseDiveSpeed * Time.deltaTime);

        
        // Compute vertical component based on dive angle

        float diveRadians = Mathf.Deg2Rad * currentDiveAngle;
        float horizontalMag = Mathf.Cos(diveRadians) * speed;
        float verticalMag = -Mathf.Sin(diveRadians) * speed;

        Vector3 finalVel = horizontalVelocity.normalized * horizontalMag;
        finalVel.y = verticalMag;

        rb.linearVelocity = finalVel;

        
        //extra gravity (not needed)
        float gravityMultiplier = Mathf.Lerp(glideGravityMultiplier, diveGravityMultiplier, currentDiveAngle / maxNoseDiveAngle);
        
        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

    }

    private void HandleWallRun()
    {
        if(wallHit is RaycastHit hit)
        {
            
            if(Input.GetMouseButton(0) && stickLeft > 0)
            {
                Debug.Log("Stick?");
                //rb.useGravity = false; // Turn off gravity
                isSticking = true;

                stickLeft -= stickDepletionRate * Time.deltaTime;
                stickLeft = Mathf.Max(0, stickLeft);


                Vector3 wallNormal = currentSurfaceNormal;
                Vector3 wallUp = Vector3.Cross(Vector3.up, wallNormal).normalized;
                Vector3 wallRight = Vector3.Cross(wallUp, wallNormal).normalized;

                if (slideActive)
                {
                    rb.useGravity = false;

                    rb.AddForce(Physics.gravity * slideGravityMultiplier, ForceMode.Acceleration);

                    // Camera-relative movement constrained to wall
                    Vector3 camForward = cameraTransform.forward;
                    Vector3 camRight = cameraTransform.right;

                    Vector3 moveForward = Vector3.ProjectOnPlane(camForward, wallNormal).normalized;
                    Vector3 moveRight = Vector3.ProjectOnPlane(camRight, wallNormal).normalized;

                    float horizontal = Input.GetAxisRaw("Horizontal");
                    float vertical = Input.GetAxisRaw("Vertical");

                    Vector3 inputDir = (moveForward * vertical + moveRight * horizontal).normalized;

                    // Keep existing velocity along wall
                    Vector3 velocityAlongWall = Vector3.ProjectOnPlane(rb.linearVelocity, wallNormal);

                    // Add small influence
                    float controlStrength = 0.2f;
                    Vector3 finalVelocity = velocityAlongWall + inputDir * controlStrength;

                    rb.linearVelocity = new Vector3(finalVelocity.x, finalVelocity.y, finalVelocity.z);
                }
                else
                {
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                }
            }
            else if(!slideActive)
            {
                //can probably get rid of this whole else if
                isSticking = false;
                rb.useGravity = true;
                onWall = false;
                wallHit = null;

                if (stickLeft < maxStickCharge && !Input.GetMouseButton(0))
                {
                    stickLeft += stickRechargeRate * Time.deltaTime;
                    stickLeft = Mathf.Min(maxStickCharge, stickLeft);
                }
            }
            
        }
        else
        {
            isSticking = false;
            rb.useGravity = true;
            onWall = false;
            wallHit = null;
            

            if (stickLeft < maxStickCharge && !Input.GetMouseButton(0))
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
        if (Input.GetKey(KeyCode.Space))
        {
            bool charged = false;
            if(chargeTime == maxChargeTime) charged = true;



            if ((onWall ||onGround) && !isSticking && Time.time - lastJumped > jumpBufferTime)
            {
                lastJumped = Time.time;
                Bounce(rb.linearVelocity, currentSurfaceNormal, charged);
                chargeTime = 0f;
                chargeHeld = false;
            }

            
            return;
        }

        //if space was pressed (keydown) && jumpCharge timer not already started
        //start charge timer
        if (Input.GetKey(KeyCode.LeftShift) && !chargeHeld)
        {
            chargeHeld = true;
            chargeTime = 0f;
        }

        if(chargeHeld && Input.GetKey(KeyCode.LeftShift))
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Min(chargeTime, maxChargeTime);
        }
    }

    void Accelerate(Vector3 direction, float acceleration, float maxMoveSpeed)
    {
 
        Vector3 velocity = rb.linearVelocity;

        float currentSpeedInDir = Vector3.Dot(velocity, direction);

        if (slideActive && isSticking)
        {
            return;
        }

        if (isGliding) return;

        //on ground pressing input
        if (onGround && !onWall)
        {
            moveOnGround(direction, velocity, acceleration, maxMoveSpeed);
        }
        

        //in air or on wall
        else if (!onGround && !onWall)
        {

            moveInAir(currentSpeedInDir, maxMoveSpeed, acceleration, direction, airFrictionStrength);
        }
        
    }


    //TODO: Balance the jumping so you cant gain inifnite speed
    void Bounce(Vector3 veloctiy, Vector3 surfaceNormal, bool charged) 
    {
        

        float jumpSpeed = baseJumpMagnitude;
        if(preImpactVelocity.magnitude > baseJumpMagnitude)
        {
            jumpSpeed = preImpactVelocity.magnitude;
        }

        

        //get rid of velocity into the surface
        float intoSurface = Vector3.Dot(rb.linearVelocity, currentSurfaceNormal);
        if (intoSurface < 0f) 
        {
            rb.linearVelocity -= intoSurface * currentSurfaceNormal;
        }

        Debug.Log("YO");
        float maxAngle = 80f;
        Vector3 newVelocity;
        Vector3 camDir = cameraTransform.forward.normalized;

        if (charged)
        {
            float angle = Vector3.Angle(camDir, currentSurfaceNormal);

            if(angle > maxAngle)
            {
                camDir = Vector3.RotateTowards(currentSurfaceNormal, camDir, Mathf.Deg2Rad * maxAngle, 0f);
            }

            camDir.Normalize();

            float speed = veloctiy.magnitude;
            Debug.Log(speed);
            Debug.Log(camDir);
            newVelocity = camDir * (speed + jumpSpeed);

            rb.linearVelocity = newVelocity;

            return;
        }

        //need to add non charged jump
        newVelocity = baseJumpMagnitude * currentSurfaceNormal.normalized;
        rb.linearVelocity += newVelocity;

        //Glide r/q
        //Slide RMB
        //Shift charge jump
        //space jump



    }



    private void moveInAir(float currentSpeedInDir, float maxMoveSpeed, float acceleration, Vector3 direction, float friction)
    {
        Vector3 accelDir = direction.normalized;

        float currentSpeed = Vector3.Dot(rb.linearVelocity, accelDir);

        
        float addSpeed = maxMoveSpeed - currentSpeed;

        if (addSpeed < 0f) return;

        float accelSpeed = acceleration * Time.deltaTime;
        if (accelSpeed > addSpeed) accelSpeed = addSpeed;

        
        // Apply acceleration along accelDir
        rb.linearVelocity += accelDir * accelSpeed;

        rb.AddForce(Physics.gravity * airGravityMultiplier, ForceMode.Acceleration);

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 frictionForce = airFrictionStrength * Time.deltaTime * -horizontalVel;
        rb.linearVelocity += frictionForce;

    }


    private void moveOnGround(Vector3 direction, Vector3 velocity, float acceleration, float maxMoveSpeed)
    {
        float friction = 8f;
        float accel = groundAcceleration; // smooth accel value for ground movement
        Vector3 newVelocity = velocity;

        if (slideActive)
        {
            
            Vector3 accelDir = direction.normalized;

            float currentSpeed = Vector3.Dot(rb.linearVelocity, accelDir);

            
            float addSpeed = maxMoveSpeed * groundSlideSpeedMultiplier - currentSpeed;

            if (addSpeed < 0f) return;

            float accelSpeed = acceleration * Time.deltaTime;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            
            // Apply acceleration along accelDir
            rb.linearVelocity += accelDir * accelSpeed;

            
        }
        else
        {
            // Normal ground control
            Vector3 targetVel = direction.normalized * maxInputSpeed;

            //if there is input change velocity
            if (direction.normalized != Vector3.zero) 
            {
                rb.linearVelocity = new Vector3(targetVel.x, velocity.y, targetVel.z);
            }

            // Apply friction
            Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
            Vector3 counterForce = friction * Time.fixedDeltaTime * -horizontalVel; 
            rb.AddForce(counterForce, ForceMode.VelocityChange);
            
        }
    }
    
    private void startSlide()
    {
        col.material = slideFriction;
    }

    private void stopSlide()
    {
        col.material = normalFriction;
    }
}







