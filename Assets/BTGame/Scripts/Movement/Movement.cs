using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private float accel = 200f;         // How fast the player accelerates on the ground
    [SerializeField]
    private float airAccel = 200f;      // How fast the player accelerates in the air
    [SerializeField]
    private float maxSpeed = 6.4f;      // Maximum player speed on the ground
    [SerializeField]
    private float maxAirSpeed = 0.6f;   // "Maximum" player speed in the air
    [SerializeField]
    private float friction = 8f;        // How fast the player decelerates on the ground
    [SerializeField]
    private float jumpForce = 5f;       // How high the player jumps
    [SerializeField]
    private LayerMask groundLayers;

    [SerializeField]
    private float rotationSpeed = 10f;

    [Header("Dashing")]
    [SerializeField]
    private float dashHeight = 10f;
    [SerializeField]
    private float dashDistance = 5f;

    [SerializeField]
    private GameObject camObj;

    [Header("Sounds")]
    [SerializeField]
    private AudioSource[] footStepSounds;
    [SerializeField]
    private int currentStepIndex = 0;
    [SerializeField]
    private float footStepDelay = .5f;

    [SerializeField]
    private AudioSource jumpSound;
    [SerializeField]

    private float stepTimer = 0f;
    private float lastJumpPress = -1f;
    private float jumpPressDuration = 0.1f;

    private bool onGround = false;

    private bool isDashing = false;
    
    private void Update()
    {
        float inputX = Input.GetAxis("Mouse X");
        Vector3 euler = transform.eulerAngles;
        euler.y += inputX * rotationSpeed;
        transform.eulerAngles = euler;

        //print(new Vector3(GetComponent<Rigidbody>().velocity.x, 0f, GetComponent<Rigidbody>().velocity.z).magnitude);
        if (Input.GetButton("Jump"))
        {
            lastJumpPress = Time.time;
        }

        // If the player is moving
        if (isMoving)
        {
            // Count up the step timer
            stepTimer += Time.deltaTime;

            // If timer reaches delay
            if (stepTimer >= footStepDelay)
            {
                // Increase stepIndex
                currentStepIndex++;

                // If stepIndex reaches the end
                if (currentStepIndex >= footStepSounds.Length)
                {
                    // Reset step index to zero
                    currentStepIndex = 0;
                }

                // Play sound at index
                footStepSounds[currentStepIndex].Play();
                // Reset the timer
                stepTimer = 0f;
            }
        }
    }

    KeyCode pressedKey;
    float timer = 0f;
    float delay = .5f;

    bool isPressed = false;
    bool isDoubleTapped = false;
    bool isMoving = false;
    void OnGUI()
    {
        //if (isMoving)
        //{
            Event e = Event.current;

            // Is key down and key is not the same as before
            // Store key pressed

            // If key pressed is down again (within time frame)
            // If key the same as before
            // Double tap!

            // A key has been released
            if (e.type == EventType.KeyUp)
            {
                // Nothing's pressed
                isPressed = false;
            }

            // 
            timer += Time.deltaTime;
            if (timer <= delay)
            {
                if (e.type == EventType.KeyDown &&
                    e.keyCode == pressedKey)
                {
                    Debug.Log("Double Tapped!");
                    isDoubleTapped = true;
                }
            }

            // If a key is down && another key is not pressed
            if (e.type == EventType.KeyDown && !isPressed)
            {
                // Something is pressed
                isPressed = true;
                // Record current key pressed (for later)
                pressedKey = e.keyCode;
                // Reset timer to detect double pressed
                timer = 0f;
            }
        //}
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        float inputH = Input.GetAxis("Horizontal") * dashDistance;
        float inputV = Input.GetAxis("Vertical") * dashDistance;
        Vector3 force = transform.right * inputH + transform.forward * inputV;
        force += transform.up * dashHeight;

        Gizmos.DrawLine(transform.position, transform.position + force);
    }
    
    private void FixedUpdate()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if(input.magnitude > 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        // Get player velocity
        Vector3 playerVelocity = GetComponent<Rigidbody>().velocity;
        // Slow down if on ground
        playerVelocity = CalculateFriction(playerVelocity);
        // Add player input
        playerVelocity += CalculateMovement(input, playerVelocity);
        
        // And the player pressed again, then you're dashing!
        isDashing = isDoubleTapped;
        
        // Is dashing?
        if (isDashing && CheckGround())
        {
            float inputH = Input.GetAxis("Horizontal") * dashDistance;
            float inputV = Input.GetAxis("Vertical") * dashDistance;
            Vector3 force = transform.right * inputH + transform.forward * inputV;
            force += transform.up * dashHeight;
            playerVelocity = force;

            isDashing = false;
            isDoubleTapped = false;
        }

        // Assign new velocity to player object
        GetComponent<Rigidbody>().velocity = playerVelocity;
    }

    /// <summary>
    /// Slows down the player if on ground
    /// </summary>
    /// <param name="currentVelocity">Velocity of the player</param>
    /// <returns>Modified velocity of the player</returns>
	private Vector3 CalculateFriction(Vector3 currentVelocity)
    {
        onGround = CheckGround();
        float speed = currentVelocity.magnitude;

        if (!onGround || Input.GetButton("Jump") || speed == 0f)
            return currentVelocity;

        float drop = speed * friction * Time.deltaTime;
        return currentVelocity * (Mathf.Max(speed - drop, 0f) / speed);
    }

    /// <summary>
    /// Moves the player according to the input. (THIS IS WHERE THE STRAFING MECHANIC HAPPENS)
    /// </summary>
    /// <param name="input">Horizontal and vertical axis of the user input</param>
    /// <param name="velocity">Current velocity of the player</param>
    /// <returns>Additional velocity of the player</returns>
    private Vector3 CalculateMovement(Vector2 input, Vector3 velocity)
    {
        onGround = CheckGround();

        //Different acceleration values for ground and air
        float curAccel = accel;
        if (!onGround)
            curAccel = airAccel;

        //Ground speed
        float curMaxSpeed = maxSpeed;

        //Air speed
        if (!onGround)
            curMaxSpeed = maxAirSpeed;

        //Get rotation input and make it a vector
        Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, 0f);
        Vector3 inputVelocity = Quaternion.Euler(camRotation) *
                                new Vector3(input.x * curAccel, 0f, input.y * curAccel);

        //Ignore vertical component of rotated input
        Vector3 alignedInputVelocity = new Vector3(inputVelocity.x, 0f, inputVelocity.z) * Time.deltaTime;

        //Get current velocity
        Vector3 currentVelocity = new Vector3(velocity.x, 0f, velocity.z);

        //How close the current speed to max velocity is (1 = not moving, 0 = at/over max speed)
        float max = Mathf.Max(0f, 1 - (currentVelocity.magnitude / curMaxSpeed));

        //How perpendicular the input to the current velocity is (0 = 90°)
        float velocityDot = Vector3.Dot(currentVelocity, alignedInputVelocity);

        //Scale the input to the max speed
        Vector3 modifiedVelocity = alignedInputVelocity * max;

        //The more perpendicular the input is, the more the input velocity will be applied
        Vector3 correctVelocity = Vector3.Lerp(alignedInputVelocity, modifiedVelocity, velocityDot);

        //Apply jump
        correctVelocity += GetJumpVelocity(velocity.y);

        //Return
        return correctVelocity;
    }

    /// <summary>
    /// Calculates the velocity with which the player is accelerated up when jumping
    /// </summary>
    /// <param name="yVelocity">Current "up" velocity of the player (velocity.y)</param>
    /// <returns>Additional jump velocity for the player</returns>
	private Vector3 GetJumpVelocity(float yVelocity)
    {
        Vector3 jumpVelocity = Vector3.zero;

        if (Time.time < lastJumpPress + jumpPressDuration && yVelocity < jumpForce && CheckGround())
        {
            lastJumpPress = -1f;
            jumpVelocity = new Vector3(0f, jumpForce - yVelocity, 0f);
        }

        return jumpVelocity;
    }

    /// <summary>
    /// Checks if the player is touching the ground. This is a quick hack to make it work, don't actually do it like this.
    /// </summary>
    /// <returns>True if the player touches the ground, false if not</returns>
    private bool CheckGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        bool result = Physics.Raycast(ray, GetComponent<Collider>().bounds.extents.y + 0.1f, groundLayers);
        return result;
    }
}
