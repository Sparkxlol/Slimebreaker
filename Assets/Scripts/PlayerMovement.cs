
using TMPro;
using Unity.Cinemachine;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;



public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody rb;
    public Transform cameraTransform;
    public Transform visualTransform;

    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera cmCamera;
    [SerializeField] private float minFov = 60;
    [SerializeField] private float maxFov = 100;
    [SerializeField] private float fovChangeSpeed = 5f;
    [SerializeField] private float positionLerpSpeed = 5f;
    [SerializeField] private float playerSpeed;
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0, 3f, -5);
    [SerializeField] private Camera playerCamera;
    

    [Header("Player Visibility")]
    [SerializeField] private Renderer[] playerRenderers; 
    [SerializeField] private float firstPersonBlendThreshold = 0.95f;

    [Header("Movement Settings")]
    public float airGravityMultiplier = 3;
    public float groundAcceleration = 20f;
    public float airAcceleration = 15f;
    public float maxInputSpeed = 20f;
    
    [SerializeField] public float wallFrictionStrength = 5f;
    public float airFrictionStrength = 1.5f;

    [Header("Jump Settings")]
    public float minJumpForce = 5f;
    public float maxJumpForce = 30f;
    public float baseJumpMagnitude = 40f;
    
    public float lastGrounded;
    public float jumpBufferTime = 0.2f;
    public float lastJumped;
    [SerializeField] private bool chargeHeld = false;
    public float chargeTime = 0f;
    public float maxChargeTime = 2f;
    [SerializeField] private float chargeJumpMultiplier = 10f;

    
    
    
    

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
    public float groundSlideSpeedMultiplier = 2f;

    [Header("Collision Checks")]
    private bool isOnSurface;
    private bool onGround = false;
    [SerializeField] private float wallRadiusCheck = 1.5f;
    private bool onWall = false;
    [SerializeField] private bool onSurface;
    private RaycastHit? wallHit = null;

    [Header("Empowered Abilities")]
    [SerializeField] public bool empoweredReady = false;
    [SerializeField] public bool empoweredSlide = false;
    [SerializeField] public bool empoweredJump = false;
    [SerializeField] public bool empoweredStick = false;
    [SerializeField] public bool empoweredGlide = false;

    private bool usedEmpoweredSlide = false;
    private bool usedEmpoweredStick = false;

    private int slideNum = 1;
    private int jumpNum = 2;
    private int stickNum = 3;
    private int glideNum = 4;

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
    private bool currentSurfaceSticky = false;

    private Collider col;
    public PhysicsMaterial normalFriction;
    public PhysicsMaterial slideFriction;
    
    bool justBounced;

    Vector3 preImpactVelocity;

    private void Start()
    {
        col = GetComponentInChildren<Collider>();
        if (col == null) Debug.LogError("No collider found under PlayerRoot!");
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


        if (isSticking && !slideActive)
        {
            rb.useGravity = false;
            rb.constraints |= RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
        }

        HandleSlide();
        HandleMovementInput();
        //CheckGrounded();
        //CheckWalled();
        //CheckGrounded();
        //CheckWalled();
        CheckSurface();
        //cameraControl();
        HandleJumpInput();
    }

    //void cameraControl()
    //{

    //    if (playerCamera == null || cmCamera == null)
    //    {
    //        Debug.Log("camera null");
    //        return;
    //    }

    //    // --- Speed-based FOV ---
    //    float speed = rb.linearVelocity.magnitude;
    //    float speedFov = Mathf.Lerp(minFov, maxFov, speed / maxInputSpeed);

    //    // --- First-person FOV ---
        

    //    //Hide player model when fully first-person
    //    bool hidePlayer = chargeBlend >= firstPersonBlendThreshold;
    //    foreach (Renderer r in playerRenderers)
    //    {
    //        if (r != null) r.enabled = !hidePlayer;
    //    }

    //}

    void FixedUpdate()
    {
        isOnSurface = onGround;
        if (!isOnSurface) 
        {
            isOnSurface = isSticking;
        }

        if (isSticking && !slideActive)
        {
            rb.useGravity = false;
            rb.constraints |= RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
        }


        float accel = isOnSurface ? groundAcceleration : airAcceleration;

        Vector3 moveOnSurface = Vector3.ProjectOnPlane(inputDirection, Vector3.up).normalized;

        //STOP acceleration for a frame after bounce
        


        

        HandleWallRun();
        HandleGlide(moveOnSurface);

        if(Time.time - lastJumped > 0.1f)
        {
            justBounced = false;
        }

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
                usedEmpoweredSlide = true;
                startSlide();
                slideActive = true;
            }
            else
            {
                stopEmpowered(slideNum);
                stopSlide();
                slideActive= false;
            }

            
        }
        else
        {
            //wont get rid of empowered slide if never pressed slide key
            if(usedEmpoweredSlide)
            {
                stopEmpowered(slideNum);
            }

            stopSlide();
            slideActive = false;
            slideLeft += slideRechargeRate * Time.deltaTime;
            slideLeft = Mathf.Min(maxSlideCharge, slideLeft);
        }
    }

    //private void CheckGrounded()
    //{
    //    // Casts a downward raycast and checks if the tag Ground is applied.
    //    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundDistanceCheck))
    //    {
    //        if (hit.collider.CompareTag("Ground"))
    //        {
    //            Debug.DrawRay(transform.position, Vector3.down * (groundDistanceCheck), Color.red, .1f);

    //            onGround = true;
    //            currentSurfaceNormal = hit.normal;

    //            //Start timer to give buffer for jump
    //            lastGrounded = Time.time;
    //            preImpactVelocity = rb.linearVelocity;

    //            return;
    //        }
    //    }
    //    else
    //    {
    //        Debug.DrawRay(transform.position, Vector3.down * (groundDistanceCheck), Color.green, .1f);
    //    }

    //    onGround = false;



    //}

    //private void CheckWalled()
    //{
    //    int mask = ~LayerMask.GetMask("Player");

    //    // Casts a circle towards the player's direction and checks if touching a sticky surface.


    //    RaycastHit hit;

    //    // If previously onWall, cast towards the previousHit's direction.
    //    if (onWall)
    //    {
    //        if (Physics.SphereCast(transform.position + new Vector3(0, wallRadiusCheck, 0), wallRadiusCheck, wallHitDirection, out hit, wallRadiusCheck))
    //        {

    //            if (hit.collider.GetComponent<StickySurface>())
    //            {
    //                //Debug.Log("On Wall");
    //                lastGrounded = Time.time;
    //                currentSurfaceNormal = hit.normal;
    //                preImpactVelocity = rb.linearVelocity;

    //                wallHit = hit;
    //                onWall = true;

    //                return;
    //            }
    //            //if not sticky surface, make wallhit and onwall no?
    //        }
    //        else
    //        {
    //            Debug.Log("OFF WALL");
    //            onGround = false;
    //            wallHit = null;
    //            onWall = false;
    //        }




    //    }



    //    // Check if a wall is nearby towards current velocity's direction.
    //    if (Physics.SphereCast(transform.position + new Vector3(0, wallRadiusCheck, 0), wallRadiusCheck, rb.linearVelocity.normalized, out hit, wallRadiusCheck))
    //    {
    //        if (hit.collider.GetComponent<StickySurface>())
    //        {
    //            //Debug.Log("On Wall 2");
    //            currentSurfaceNormal = hit.normal;
    //            lastGrounded = Time.time;
    //            preImpactVelocity = rb.linearVelocity;

    //            wallHit = hit;
    //            onWall = true;

    //            wallHitDirection = rb.linearVelocity.normalized;
    //        }
    //    }

    //}

    



    private void CheckSurface()
    {
        Collider[] hitcolliders = new Collider[10];


        Collider childCol = GetComponentInChildren<Collider>();
        if (childCol == null)
        {
            Debug.Log("Child collider null");
            return;
        }
        
        Vector3 checkOrigin = childCol.bounds.center;


        int hits = Physics.OverlapSphereNonAlloc(checkOrigin, wallRadiusCheck, hitcolliders, surfaceLayer);


        onGround = false;
        onWall = false;
        onSurface = hits > 0;
        wallHit = null;
        currentSurfaceSticky = false;



        if (onSurface)
        {
            float closestDist = Mathf.Infinity;
            Vector3 closestPoint = Vector3.zero;
            Collider closestCollider = null;
            

            for (int i = 0; i < hits; i++)
            {
                Collider c = hitcolliders[i];
                if (c == null) continue;

                Vector3 point = c.ClosestPoint(checkOrigin);
                float dist = Vector3.Distance(checkOrigin, point);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPoint = point;
                    closestCollider = c;
                }


            }

            if (closestDist < Mathf.Infinity)
            {
                Vector3 dir = (closestPoint - checkOrigin).normalized;
                if (Physics.Raycast(checkOrigin, dir, out RaycastHit hit, wallRadiusCheck * 2f, surfaceLayer))
                {
                    currentSurfaceNormal = hit.normal;

                    if (hit.collider.GetComponent<StickySurface>())
                    {

                        currentSurfaceSticky = hit.collider.GetComponent<StickySurface>() ? true : false;
                        

                        currentSurfaceNormal = hit.normal;
                        lastGrounded = Time.time;
                        preImpactVelocity = rb.linearVelocity;

                        wallHit = hit;
                        onWall = true;
                        onGround = false;
                        return;
                    }
                    else if (hit.collider.CompareTag("Ground"))
                    {
                        currentSurfaceSticky = false;
                        onGround = true;
                        onWall = false;
                        //Start timer to give buffer for jump
                        lastGrounded = Time.time;
                        preImpactVelocity = rb.linearVelocity;
                    }
                }

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
                stopEmpowered(glideNum);
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

            stopEmpowered(glideNum);
            rb.useGravity = true;
            isGliding = false;
            return;
        }

        // If gliding
        if (isGliding && glideLeft > 0 && (!onGround || !onWall))
        {
            if (rb.linearVelocity.y > 0)
            {
                glideLeft -= glideDepletionRate * Time.deltaTime;
                rb.AddForce(Physics.gravity * (slowDownGravityMultiplier), ForceMode.Acceleration);

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
            //end gliding
            stopEmpowered(glideNum);
            isGliding = false;
            rb.useGravity = true;
        }
    }

    private void glideInput(Vector3 inputDir)
    {
        float tempGlideDepletion = glideDepletionRate;
        float tempGlideGravityMulti = glideGravityMultiplier;

        if(empoweredGlide)
        {
            tempGlideDepletion /= empoweredGlideMultiplier;
            tempGlideGravityMulti /= empoweredGlideMultiplier;
        }

        glideLeft -= tempGlideDepletion * Time.deltaTime;
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


        
        bool pressingW = false;

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


        //extra gravity
        float gravityMultiplier = Mathf.Lerp(tempGlideGravityMulti, diveGravityMultiplier, currentDiveAngle / maxNoseDiveAngle);

        rb.AddForce(Physics.gravity * glideGravityMultiplier, ForceMode.Acceleration);

    }

    private void HandleWallRun()
    {
        if(wallHit is RaycastHit hit)
        {
            


            if (Input.GetMouseButton(0) && stickLeft > 0)
            {
                
                
                isSticking = true;


                float trueStickDepletionRate = stickDepletionRate;
                if(empoweredStick)
                {
                    usedEmpoweredStick = true;
                    trueStickDepletionRate /= empoweredStickMultiplier;
                }

                stickLeft -= trueStickDepletionRate * Time.deltaTime;
                stickLeft = Mathf.Max(0, stickLeft);


                Vector3 wallNormal = currentSurfaceNormal;
                Vector3 wallUp = Vector3.Cross(Vector3.up, wallNormal).normalized;
                Vector3 wallRight = Vector3.Cross(wallUp, wallNormal).normalized;

                if (slideActive)
                {
                    rb.useGravity = false;
                    

                    float trueSlideGravityMultiplier = slideGravityMultiplier;
                    if(empoweredSlide)
                    {
                        trueSlideGravityMultiplier /= empoweredSlideMultiplier;
                    }

                    rb.AddForce(Physics.gravity * trueSlideGravityMultiplier, ForceMode.Acceleration);

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
                    if(empoweredSlide)
                    {
                        controlStrength *= empoweredSlideMultiplier;
                    }

                    Vector3 finalVelocity = velocityAlongWall + inputDir * controlStrength;

                    rb.linearVelocity = new Vector3(finalVelocity.x, finalVelocity.y, finalVelocity.z);
                }
                else
                {
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                }
            }
            else
            {
                
                isSticking = false;
                rb.useGravity = true;
                
                wallHit = null;
                
                if(usedEmpoweredStick)
                {
                    stopEmpowered(stickNum);
                    usedEmpoweredStick = false;
                }

                if (stickLeft < maxStickCharge && !Input.GetMouseButton(0))
                {
                    stickLeft += stickRechargeRate * Time.deltaTime;
                    stickLeft = Mathf.Min(maxStickCharge, stickLeft);
                }
            }
            
        }

        if(!Input.GetMouseButton(0) || !onWall)
        {
            isSticking = false;
            rb.useGravity = true;
            
            wallHit = null;
        }
        


        if (stickLeft < maxStickCharge && !Input.GetMouseButton(0))
        {
            stickLeft += stickRechargeRate * Time.deltaTime;
            stickLeft = Mathf.Min(maxStickCharge, stickLeft);
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

                stopEmpowered(jumpNum);
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

        if(!Input.GetKey(KeyCode.LeftShift))
        {
            chargeTime = 0; 
            chargeHeld = false;
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
        if (isSticking) return;

        //on ground pressing input
        if (onGround && !onWall)
        {
            moveOnGround(direction, velocity, acceleration, maxMoveSpeed);
        }
        

        //in air
        else if (!onGround)
        {

            moveInAir(currentSpeedInDir, maxMoveSpeed, acceleration, direction, airFrictionStrength);
        }
        
    }


    //TODO: Balance the jumping so you cant gain inifnite speed
    void Bounce(Vector3 veloctiy, Vector3 surfaceNormal, bool charged) 
    {
        justBounced = true;
        

        float jumpSpeed = baseJumpMagnitude;
        if(preImpactVelocity.magnitude > baseJumpMagnitude)
        {
            jumpSpeed = preImpactVelocity.magnitude + baseJumpMagnitude;
        }

        float empoweredNonCharged = 1;
        if(empoweredJump)
        {
            jumpSpeed = empoweredJumpMultiplier * jumpSpeed;
            empoweredNonCharged *= empoweredJumpMultiplier;
        }

        //get rid of velocity into the surface
        float intoSurface = Vector3.Dot(rb.linearVelocity, currentSurfaceNormal);
        if (intoSurface < 0f) 
        {
            rb.linearVelocity -= intoSurface * currentSurfaceNormal;
        }

        
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
            Debug.Log(jumpSpeed);
            //jumpSpeed += chargeJumpMultiplier;
            newVelocity = camDir * (speed + jumpSpeed);
            Debug.Log(newVelocity);

            rb.linearVelocity = newVelocity;

            return;
        }

        //need to add non charged jump
        newVelocity = baseJumpMagnitude * empoweredNonCharged * currentSurfaceNormal.normalized;
        rb.linearVelocity += newVelocity;

 
    }



    private void moveInAir(float currentSpeedInDir, float maxMoveSpeed, float acceleration, Vector3 direction, float friction)
    {
        
        Vector3 accelDir = direction.normalized;

        //make sure no sticking to wall
        float gravityMultiplier = airGravityMultiplier;
        if (onWall)
        {
            float dotIntoWall = Vector3.Dot(accelDir, currentSurfaceNormal);

            if(dotIntoWall < -.5f)
            {
                
                gravityMultiplier *= 3;
                
            }
            else
            {
                gravityMultiplier *= 1.5f;
            }
            
            

        }



        float currentSpeed = Vector3.Dot(rb.linearVelocity, accelDir);

        
        float addSpeed = maxMoveSpeed - currentSpeed;

        if (addSpeed < 0f) return;

        float accelSpeed = acceleration * Time.deltaTime;
        if (accelSpeed > addSpeed) accelSpeed = addSpeed;

        
        // Apply acceleration along accelDir
        rb.linearVelocity += accelDir * accelSpeed;

        //prevent sticking to wall if pressing into wall
        

        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

        //Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        //Vector3 frictionForce = airFrictionStrength * Time.deltaTime * -horizontalVel;
        //Vector3 frictionForce = -horizontalVel.normalized * Mathf.Min(horizontalVel.magnitude, airFrictionStrength * Time.deltaTime);
        //rb.linearVelocity += frictionForce;

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

            //if empowered slide add multi
            float tempGroundSlideSpeedMulti = groundSlideSpeedMultiplier;
            if(empoweredSlide)
            {
                tempGroundSlideSpeedMulti *= empoweredSlideMultiplier;
            }

            
            float addSpeed = maxMoveSpeed * tempGroundSlideSpeedMulti - currentSpeed;

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

    private void stopEmpowered(int num)
    {
        switch (num)
        {
            case 1:
                if (empoweredSlide)
                {
                    usedEmpoweredSlide = false;
                    empoweredReady = false;
                    empoweredSlide = false; 
                }
                break;
            case 2:
                if (empoweredJump)
                {
                    empoweredReady = false;
                    empoweredJump = false;
                }
                break;
            case 3:
                if (empoweredStick)
                {
                    empoweredReady = false;
                    empoweredStick = false;
                }
                break;
            case 4:
                if (empoweredGlide)
                {
                    empoweredReady = false;
                    empoweredGlide = false;
                }
                break;

            default: break;
        }
    }
}







