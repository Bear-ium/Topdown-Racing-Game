using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float accelerationPower = 5f;
    public float steeringPower = 5f;
    public float maxSpeed = 30f;
    public float driftFactor = 0.9f;
    public float handbrakeDriftFactor = 0.5f;
    public float traction = 0.9f;

    [Header("Gear Settings")]
    public int maxGear = 6;
    public float[] gearSpeeds; // Speed limits for each gear, e.g., { 5, 10, 15, 20, 25, 30 }

    private Rigidbody2D rb;
    private float steeringInput;
    private float accelerationInput;
    private float rotationAngle;
    private int currentGear = 1;
    private bool clutchDown = false;
    private bool handbrakeDown = false;

    [Header("Controls / Keybinds")]
    public KeyCode clutchKey;
    public KeyCode handbreakKey;
    public KeyCode moveGearShiftUp;
    public KeyCode moveGearShiftDown;
    public KeyCode moveGearShiftLeft;
    public KeyCode moveGearShiftRight;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;  // Disable gravity for top-down racing game

        // Initialize gear speed limits
        if (gearSpeeds.Length < maxGear + 1)
        {
            gearSpeeds = new float[maxGear + 1];
            for (int i = 1; i <= maxGear; i++)
            {
                gearSpeeds[i] = maxSpeed / maxGear * i;
            }
            gearSpeeds[0] = -10; // Reverse gear speed limit
        }
    }

    void Update()
    {
        // Get input for acceleration, steering, handbrake, and clutch
        accelerationInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        handbrakeDown = Input.GetKey(KeyCode.Space);
        clutchDown = Input.GetKey(KeyCode.LeftShift);

        // Handle gear shifting when clutch is pressed
        if (clutchDown)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && currentGear < maxGear)
            {
                currentGear++;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && currentGear > -1)
            {
                currentGear--;
            }
        }
    }

    void FixedUpdate()
    {
        ApplyEngineForce();
        ApplySteering();
        ApplyDrift();
        LimitMaxSpeed();
    }

    void ApplyEngineForce()
    {
        // Only apply force if the clutch is not down
        if (clutchDown) return;

        float gearMultiplier = (currentGear == 0) ? -1 : 1; // Reverse gear multiplier
        float speedLimit = (currentGear == 0) ? gearSpeeds[0] : gearSpeeds[currentGear];

        // Calculate forward force based on gear
        Vector2 engineForce = transform.up * accelerationInput * accelerationPower * gearMultiplier;
        rb.AddForce(engineForce, ForceMode2D.Force);

        // Clamp speed to the gear's speed limit
        if (rb.velocity.magnitude > speedLimit)
        {
            rb.velocity = rb.velocity.normalized * speedLimit;
        }
    }

    void ApplySteering()
    {
        // Steering only affects car if it's moving
        if (rb.velocity.magnitude > 0.1f)
        {
            float speedFactor = rb.velocity.magnitude / maxSpeed;
            float steerAmount = steeringInput * steeringPower * (1.0f - speedFactor);
            rotationAngle -= steerAmount;
            rb.MoveRotation(rotationAngle);
        }
    }

    void ApplyDrift()
    {
        // Use stronger drift factor when handbrake is applied
        float currentDriftFactor = handbrakeDown ? handbrakeDriftFactor : driftFactor;

        // Separate forward and sideways velocity to simulate drift
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);

        rb.velocity = forwardVelocity + rightVelocity * currentDriftFactor;

        // Apply extra traction when accelerating/decelerating
        if (accelerationInput != 0)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, forwardVelocity, traction * Time.fixedDeltaTime);
        }
    }

    void LimitMaxSpeed()
    {
        // Clamp total velocity to the maximum speed
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
    }
}
