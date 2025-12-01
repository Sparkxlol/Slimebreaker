
using TMPro;
using Unity.Cinemachine;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;



public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody rb;
    public Transform cameraTransform;
    public Transform visualTransform;
    private Transform modelTransform;

    [Header("Animations")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool walking;
    [SerializeField] private bool inAir;
    [SerializeField] private bool jumping;


    [Header("Movement Settings")]
    public float airGravityMultiplier = 5;
    public float groundAcceleration = 40f;
    public float airAcceleration = 15f;
    public float maxInputSpeed = 20f;
    
    [SerializeField] public float wallFrictionStrength = 5f;
    public float airFrictionStrength = 1.5f;

    [Header("Jump Settings")]
    
    public float baseJumpMagnitude = 40f;
    
    public float lastGrounded;
    public float jumpBufferTime = 0.2f;
    public float lastJumped;
    [SerializeField] private bool chargeHeld = false;
    public float chargeTime = 0f;
    public float maxChargeTime = 0.4f;
    [SerializeField] private bool chargeReleased = true;
    private bool pressedSpace = false;

    
    
    
    

    [Header("Stickiness")]
    public float stickLeft;
    public bool isSticking;
    public float maxStickCharge = 7f;
    public float stickDepletionRate = 1f;
    public float stickRechargeRate = 1.5f;

    [Header("Glide")]
    public float glideLeft;
    public bool isGliding;
    public float maxGlideCharge = 10f;
    public float glideRechargeRate = 1.5f;
    public float glideDepletionRate = 1f;
    public float glideGravityMultiplier = 12f;
    public float diveGravityMultiplier = 25f;
    [SerializeField] private float currentVerticalDiveAngle = 0f;
    [SerializeField] private float currentHorizontalDiveAngle = 0f;
    [SerializeField] private float defaultVerticalGlideAngle = -10f;
    [SerializeField] private float defaultHorizontalGlideAngle = 0f;
    [SerializeField] private float slowDownGravityMultiplier = 3f;
    [SerializeField] private float lastGlidePress = 0f;
    [SerializeField] public bool glideTogglePressed = false;
    [SerializeField] private Vector3 initialGlideVelocity;
    [SerializeField] private bool glideJustStarted;
    private float maxClimbSpeed = 0;

    [Header("Slide")]
    public bool slideActive;
    public float slideLeft;
    public float maxSlideCharge = 5f;
    public float slideDepletionRate = 1f;
    public float slideRechargeRate = 1f;
    public float slideGravityMultiplier = 0.5f;
    public float groundSlideSpeedMultiplier = 2f;

    [Header("Collision Checks")]
    public bool isOnSurface;
    public bool onGround = false;
    [SerializeField] private float wallRadiusCheck = 1.5f;
    public bool onWall = false;
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


    [Header("Sound Effects")]
    [SerializeField] private AudioClip jumpClip;

    private void Start()
    {
        col = GetComponentInChildren<Collider>();
        if (col == null) Debug.LogError("No collider found under PlayerRoot!");
        col.material = normalFriction;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        modelTransform = transform.Find("Model");
        if (modelTransform == null)
            Debug.LogError("Model child object not found!");

    }
    void Update()
    {
        // Ignores Inputs + Updates when game is paused.
        if (GameManager.GamePaused) return;


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
        CheckSurface();


        animator.SetBool("Gliding", isGliding);


        if(isGliding || inAir)
        {
            animator.SetBool("Sliding", false);
        }
        else
        {
            animator.SetBool("Sliding", slideActive);
        }
        

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

        pressedSpace = Input.GetKey(KeyCode.Space);

        HandleJumpInput();

        HandleWallRun();
        HandleGlide(moveOnSurface);

        if (Time.time - lastJumped > 0.1f)
        {
            justBounced = false;
        }

        if (!justBounced)
        {
            Accelerate(moveOnSurface, accel, maxInputSpeed);
        }

        glideTogglePressed = false;

        
        animator.SetBool("Walking", walking);

        animator.SetBool("InAir", inAir);
        

        animator.SetBool("Jump", jumping);
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
                if (c == null || c is MeshCollider) continue;
                
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
        if (!onGround && !onWall)
        {

            //need to do a simple downward raycast so player can walk on mesh collider
            RaycastHit groundHit;
            Vector3 rayOrigin = checkOrigin + Vector3.up * 0.1f; // slight lift prevents false negatives


            if (Physics.Raycast(rayOrigin, Vector3.down, out groundHit, wallRadiusCheck * 1.5f, surfaceLayer))
            {


                if (groundHit.collider.CompareTag("Floor"))
                {

                    onGround = true;
                    onWall = false;
                    currentSurfaceNormal = groundHit.normal;
                    lastGrounded = Time.time;
                    preImpactVelocity = rb.linearVelocity;
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
        //transform.eulerAngles = new Vector3(0, cameraTransform.eulerAngles.y, 0);
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

                glideJustStarted = true;
                initialGlideVelocity = rb.linearVelocity;
                maxClimbSpeed = initialGlideVelocity.magnitude;
                currentVerticalDiveAngle = defaultVerticalGlideAngle;
                currentHorizontalDiveAngle = defaultHorizontalGlideAngle;
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
            

            stopEmpowered(glideNum);
            rb.useGravity = true;
            isGliding = false;
            return;
        }

        // If gliding
        if (isGliding && glideLeft > 0 && (!onGround || !onWall))
        {
            //this stops y velocity when glide starts
            if (rb.linearVelocity.y > 0 && glideJustStarted)
            {
                glideLeft -= glideDepletionRate * Time.deltaTime;
                rb.AddForce(Physics.gravity * (slowDownGravityMultiplier), ForceMode.Acceleration);

                if (rb.linearVelocity.y < 0)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                    glideJustStarted = false;
                }
            }
            else
            {
                if (glideJustStarted) glideJustStarted = false;
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

      
        

        float maxVerticalAngle = 70f;
        float maxHorizontalAngle = 45f;

        float verticalAngleChangeMultiplier = 30f;
        float horizontalAngleChangeMultiplier = 50f;

        //tilt down
        if(Input.GetKey(KeyCode.W))
        {
            currentVerticalDiveAngle -= verticalAngleChangeMultiplier * Time.deltaTime;
            currentVerticalDiveAngle = Mathf.Max((currentVerticalDiveAngle), (-maxVerticalAngle)); 
        }

        //tilt up
        else if (Input.GetKey(KeyCode.S)) 
        {
            currentVerticalDiveAngle += verticalAngleChangeMultiplier * Time.deltaTime;
            currentVerticalDiveAngle = Mathf.Min(currentVerticalDiveAngle, maxVerticalAngle);
        }
        else
        {
            if(currentVerticalDiveAngle > defaultVerticalGlideAngle)
            {
                currentVerticalDiveAngle -= verticalAngleChangeMultiplier * Time.deltaTime;
                currentVerticalDiveAngle = Mathf.Max(currentVerticalDiveAngle, defaultVerticalGlideAngle);
            }
            else if(currentVerticalDiveAngle < defaultVerticalGlideAngle)
            {
                currentVerticalDiveAngle += verticalAngleChangeMultiplier * Time.deltaTime;
                currentVerticalDiveAngle = Mathf.Min(currentVerticalDiveAngle, defaultVerticalGlideAngle);
            }
                
        }
        //geol

        //tilt left
        if (Input.GetKey(KeyCode.A))
        {
            currentHorizontalDiveAngle -= horizontalAngleChangeMultiplier * Time.deltaTime;
            currentHorizontalDiveAngle = Mathf.Max((currentHorizontalDiveAngle), -maxHorizontalAngle);
        }
        //tilt right
        else if (Input.GetKey(KeyCode.D))
        {
            currentHorizontalDiveAngle += horizontalAngleChangeMultiplier * Time.deltaTime;
            currentHorizontalDiveAngle = Mathf.Min((currentHorizontalDiveAngle), maxHorizontalAngle);
        }
        else
        {
            if (currentHorizontalDiveAngle > defaultHorizontalGlideAngle)
            {
                currentHorizontalDiveAngle -= horizontalAngleChangeMultiplier * Time.deltaTime * 5f;
                currentHorizontalDiveAngle = Mathf.Max(currentHorizontalDiveAngle, defaultHorizontalGlideAngle);
            }
            else if (currentHorizontalDiveAngle < defaultVerticalGlideAngle)
            {
                currentHorizontalDiveAngle += horizontalAngleChangeMultiplier * Time.deltaTime * 5f;
                currentHorizontalDiveAngle = Mathf.Min(currentHorizontalDiveAngle, defaultHorizontalGlideAngle);
            }
        }



        //given currentHorizontal and vertical dive angles, calculate
        //note that forwards direction is pointing from player along the angle of currentverticaldiveangle

        //If angle is upwards, speed needs to be velocity.y needs to be positive. 
        //if angle is downwards, velocity.y need to be negative

        float speed = rb.linearVelocity.magnitude;

        Vector3 verticalVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        float horizontalSpeed = horizontalVelocity.magnitude;

        float verticalSpeed = verticalVelocity.magnitude;
        float angleRatio = Mathf.Abs(currentVerticalDiveAngle / maxVerticalAngle);

        
        float maxDiveSpeed = initialGlideVelocity.magnitude * 2f;


        float initialHorizontalSpeed = new Vector3(initialGlideVelocity.x, 0, initialGlideVelocity.z).magnitude;
        float maxHorizontalSpeed = initialHorizontalSpeed * 2f;
        if(rb.linearVelocity.magnitude > maxHorizontalSpeed)
        {
            maxHorizontalSpeed = rb.linearVelocity.magnitude;
        }

        


        //Pointing up/Climbing
        if (currentVerticalDiveAngle > 0f)
        {
            float verticalSpeedChange = 30f;

            

            if(horizontalSpeed < 10f)
            {
                maxClimbSpeed = 0f;
            }

            if (maxClimbSpeed < initialGlideVelocity.y / 5f)
            {
                isGliding = false;
                verticalVelocity.y -= verticalSpeedChange * Time.deltaTime * angleRatio;
            }
            else
            {
                verticalVelocity.y = angleRatio * maxClimbSpeed;
                maxClimbSpeed *= 1 - (Time.deltaTime / 2f);

            }


            horizontalSpeed -= Time.deltaTime * angleRatio * 20f;
            horizontalSpeed = Mathf.Max(horizontalSpeed, 0f);




        }
        //if angled more downward than defaultglide, slowly gain velocity in forwards direction
        else if (currentVerticalDiveAngle < defaultVerticalGlideAngle)
        {

            //depending on how large angle is compared to maxvertical angle increase/decrease velocity quicker
            


            verticalVelocity.y = -Mathf.Abs(maxDiveSpeed * angleRatio);
            verticalVelocity.y = Mathf.Max(verticalVelocity.y, -Mathf.Abs(maxDiveSpeed));


            horizontalSpeed += Time.deltaTime * angleRatio * 10f;
            if(maxHorizontalSpeed < 20f)
            {
                maxHorizontalSpeed = 50f;
            }

            horizontalSpeed = Mathf.Min(horizontalSpeed, maxHorizontalSpeed);


        }
        //angle is higher than -10 so lose speed
        else if(currentVerticalDiveAngle > defaultVerticalGlideAngle)
        {
           

            verticalVelocity.y = -Mathf.Abs(angleRatio * maxDiveSpeed);
            verticalVelocity.y = Mathf.Min(0f, verticalVelocity.y);

            horizontalSpeed -= Time.deltaTime * 1f;
            horizontalSpeed = Mathf.Max(horizontalSpeed, 0f);
        }
        //this is causing instant downward velocity
        else
        {
            //this line is causing instant downwards when returning to default angle
            verticalVelocity.y = -Mathf.Abs(angleRatio * maxDiveSpeed);
            verticalVelocity.y = Mathf.Min(0f, verticalVelocity.y);
        }
          





                //now calculate horizontal tilt
                Vector3 tilt = Vector3.zero;
        //if tilted left:
        if (Mathf.Abs(currentHorizontalDiveAngle) > 0.1f)
        {
            if (currentHorizontalDiveAngle < 0)
            {
                tilt = Vector3.Cross(horizontalVelocity, Vector3.up).normalized;
            }
            //if tilted right
            else if (currentHorizontalDiveAngle > 0)
            {
                tilt = Vector3.Cross(Vector3.up, horizontalVelocity).normalized;
            }
        }



        //need to add small force in direction of the tilt vector the horizontal velocity
        if (tilt != Vector3.zero)
        {
            float horizontalAngleRatio = Mathf.Abs(currentHorizontalDiveAngle / maxHorizontalAngle);
            float turnStrength = horizontalAngleRatio * Time.deltaTime;
            

            //Smoothly rotate horizontal velocity toward tilt
            horizontalVelocity = Vector3.RotateTowards(
                horizontalVelocity,   
                tilt,                 
                turnStrength,         
                0f                    
            );
        }

        
        horizontalVelocity = horizontalVelocity.normalized * horizontalSpeed;

        

        rb.linearVelocity = horizontalVelocity + verticalVelocity;
        ApplyGlideRotation(currentHorizontalDiveAngle);

    }

    private void HandleWallRun()
    {
        if(wallHit is RaycastHit hit)
        {
            


            if (Input.GetMouseButton(0) && stickLeft > 0 && currentSurfaceSticky && !pressedSpace)
            {
                Vector3 vel = rb.linearVelocity;
                //remove component pushing away from the wall
                float dot = Vector3.Dot(vel, currentSurfaceNormal);
                if (dot > 0)
                {
                    vel -= currentSurfaceNormal * dot;
                }

                rb.linearVelocity = vel;

                isSticking = true;
                rotatePlayer(Vector3.zero, true);

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

                    if (!onGround)
                    {
                        trueSlideGravityMultiplier = airGravityMultiplier;
                    }

                    float stickForce = 20f;

                    rb.AddForce(-wallNormal * stickForce, ForceMode.Acceleration);

                    //Constrain camera movement along the wall
                    Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, wallNormal).normalized;
                    Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, wallNormal).normalized;

                    float h = Input.GetAxisRaw("Horizontal");
                    float v = Input.GetAxisRaw("Vertical");

                    Vector3 inputDir = (camForward * v + camRight * h).normalized;

                    //Keep velocity along wall
                    Vector3 velocityAlongWall = Vector3.ProjectOnPlane(rb.linearVelocity, wallNormal);

                    //Add small influence from input
                    float controlStrength = 0.2f;
                    if (empoweredSlide) controlStrength *= empoweredSlideMultiplier;

                    Vector3 finalVelocity = velocityAlongWall + inputDir * controlStrength;

                    rb.linearVelocity = finalVelocity;

                    rotatePlayer(inputDir, true);
                }
                else
                {
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    float stickForce = 200f;

                    rb.AddForce(-wallNormal * stickForce, ForceMode.Acceleration);
                    
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

                if (stickLeft < maxStickCharge)
                {
                    stickLeft += stickRechargeRate * Time.deltaTime;
                    stickLeft = Mathf.Min(maxStickCharge, stickLeft);
                }
            }
            
        }

        if(!onWall)
        {
            isSticking = false;
            rb.useGravity = true;
            
            wallHit = null;
        }
        


        if (stickLeft < maxStickCharge)
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
        jumping = false;
        if (pressedSpace)
        {
            bool charged = false;
            if(chargeTime == maxChargeTime) charged = true;



            if ((onWall ||onGround) && (Time.time - lastJumped > jumpBufferTime))
            {
                jumping = true;
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
        if (Input.GetKey(KeyCode.LeftShift) && !chargeHeld && chargeReleased)
        {
            
            chargeHeld = true;
            chargeTime = 0f;
        }

        if(chargeHeld && Input.GetKey(KeyCode.LeftShift))
        {
            chargeReleased = false;
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Min(chargeTime, maxChargeTime);
        }

        if(!Input.GetKey(KeyCode.LeftShift))
        {
            chargeReleased = true;
            chargeTime = 0; 
            chargeHeld = false;
        }
    }

    void Accelerate(Vector3 direction, float acceleration, float maxMoveSpeed)
    {
        walking = false;
        inAir = false;

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

    void ApplyGlideRotation(float horizontalAngle)
    {

        Vector3 vel = rb.linearVelocity;

        //YAW toward movement direction
        Vector3 flatVel = new Vector3(vel.x, 0f, vel.z);
        if (flatVel.sqrMagnitude < 0.001f)
            return; // do nothing if not moving

        Quaternion yawRot = Quaternion.LookRotation(flatVel.normalized, Vector3.up);

        //PITCH from verticalAngle
        float verticalAngle = Mathf.Asin(vel.normalized.y) * Mathf.Rad2Deg;
        Quaternion pitchRot = Quaternion.Euler(-verticalAngle, 0f, 0f);

        //ROLL from horizontalAngle
        Quaternion rollRot = Quaternion.Euler(0f, 0f, -horizontalAngle);

        //Combine all rotations
        Quaternion targetRot = yawRot * pitchRot * rollRot;

        //Smooth the rotation
        modelTransform.rotation = Quaternion.Slerp(
            modelTransform.rotation,
            targetRot,
            8f * Time.deltaTime
        );
    }

    void rotatePlayer(Vector3 direction, bool sticking)
    {
        Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);

        if(sticking)
        {
            Vector3 forward = direction;
            if(direction == Vector3.zero)
            {
                forward = modelTransform.forward;
            }

            Vector3 surfaceUp = currentSurfaceNormal.normalized;

            // Project the forward vector onto the surface
            forward = Vector3.ProjectOnPlane(forward, surfaceUp).normalized;

            // Build the rotation using LookRotation(forward, up)
            targetRot = Quaternion.LookRotation(forward, surfaceUp);

            
            
        }

        // Smoothly rotate toward it
        modelTransform.rotation = Quaternion.Slerp(
            modelTransform.rotation,
            targetRot,
            10f * Time.deltaTime
        );
    }

    //TODO: Balance the jumping so you cant gain inifnite speed
    void Bounce(Vector3 veloctiy, Vector3 surfaceNormal, bool charged) 
    {
        justBounced = true;
        

        float jumpSpeed = baseJumpMagnitude;
        

        float empoweredNonCharged = 1;
        if (empoweredJump)
        {
            jumpSpeed = empoweredJumpMultiplier * jumpSpeed;
            empoweredNonCharged *= empoweredJumpMultiplier;
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
            
            //jumpSpeed += chargeJumpMultiplier;
            newVelocity = camDir * (speed + jumpSpeed);
            Debug.Log(newVelocity);

            rb.linearVelocity = newVelocity;

            return;
        }

        //get rid of velocity into the surface
        float intoSurface = Vector3.Dot(rb.linearVelocity, currentSurfaceNormal);
        if (intoSurface < 0f)
        {
            rb.linearVelocity -= intoSurface * currentSurfaceNormal;
        }

        //need to add non charged jump
        newVelocity = baseJumpMagnitude * empoweredNonCharged * currentSurfaceNormal.normalized * 1.3f;
        rb.linearVelocity += newVelocity;

        AudioManager.instance.PlaySFX(jumpClip);
    }



    private void moveInAir(float currentSpeedInDir, float maxMoveSpeed, float acceleration, Vector3 direction, float friction)
    {
        inAir = true;
        Vector3 accelDir = direction.normalized;

        if(direction.sqrMagnitude > 0.1f)
        {
            rotatePlayer(accelDir, false);
        }
        
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

        if (addSpeed > 0f)
        {
            float accelSpeed = acceleration * Time.deltaTime;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            
            // Apply acceleration along accelDir
            rb.linearVelocity += accelDir * accelSpeed;
        }

        

        
        

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

            rotatePlayer(accelDir, false);
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
                rotatePlayer(direction.normalized, false);
                walking = true;
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







