using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;

public class CarControllerV2 : MonoBehaviour
{
    [Header("Transmission Settings")]
    public int gear = 0;        // Current gear (1 for first gear, 0 for neutral, -1 for reverse)
    public int minGear = -1;
    public int maxGear = 6;
    // Gear Max Speeds;
    // -1 |  0  |  1  |  2  |  3  |  4  |  5  |  6
    // Array Position;
    //  0 |  1  |  2  |  3  |  4  |  5  |  6  |  7
    public float[] gearRatios = { -15, 0, 20, 30, 40, 55, 70, 90 };

    [Header("Engine Settings")]
    public float throttleInput;
    public float accelerationFactor = 100f;

    [Header("Clutch Settings")]
    public bool clutchInput;

    [Header("Speed and Physics Settings")]
    public float currentSpeed;
    public float maxSpeed = 200f;
    public float dragCoefficient = 0.02f;
    public float brakingFactor = 0.5f;

    [Header("Steering Settings")]
    public float steeringSensitivity = 1f;

    [Header("Braking Settings")]
    public bool isBraking;

    [Header("Controls / Keybinds")]
    public KeyCode clutchKey = KeyCode.LeftShift;
    public KeyCode handbreakKey = KeyCode.Space;
    public KeyCode moveForward = KeyCode.W;
    public KeyCode moveBack = KeyCode.S;
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode moveGearShiftUp = KeyCode.UpArrow;
    public KeyCode moveGearShiftDown = KeyCode.DownArrow;

    // General Variables where no Header is required!
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Disable gravity
    }

    void Update() // User Input (Steering, Acceleration, Braking)
    {
        TransmissionSwitch();
        Braking();
        Movement();
    }

    void FixedUpdate() // Physics-Based Calculations
    {
        transform.Translate(Vector3.up * currentSpeed * Time.fixedDeltaTime);
    }

    // Methods

    void TransmissionSwitch()
    {
        if (rb == null)
        {
            Debug.LogWarning("There is no Rigidbody2D on this vehicle!");
            return;
        }

        if (Input.GetKey(clutchKey) && Input.GetKeyDown(moveGearShiftUp))
        {
            if (gear + 1 <= maxGear) { gear++; }
        }
        else if (Input.GetKey(clutchKey) && Input.GetKeyDown(moveGearShiftDown))
        {
            if (gear - 1 >= minGear) { gear--; }
        }
    }

    void Movement()
    {
        if (rb == null)
        {
            Debug.LogWarning("There is no Rigidbody2D on this vehicle!");
            return;
        }

        clutchInput = Input.GetKey(clutchKey);
        throttleInput = Input.GetKey(moveForward) ? 1 : 0;

        // Determine max speed for the current gear
        float maxSpeedForGear = Mathf.Abs(gearRatios[gear + 1]);

        // Acceleration and speed control
        if (throttleInput > 0 && gear != 0 && !clutchInput)
        {
            // Reverse gear logic
            if (gear < 0) // Reverse
            {
                // Apply negative speed for reverse gear
                currentSpeed -= throttleInput * accelerationFactor * Time.deltaTime;

                // Clamp speed to maximum reverse speed
                currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeedForGear, 0);
            }
            else // Forward gears
            {
                // Increase speed based on throttle and acceleration factor
                currentSpeed += throttleInput * accelerationFactor * Time.deltaTime;

                // Clamp speed to max speed allowed by the current gear
                currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeedForGear);
            }
        }
        else
        {
            // Decelerate towards zero when no throttle is applied
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, dragCoefficient * Time.deltaTime * 1.5f);
        }

        // Backward movement control (braking or reversing)
        if (Input.GetKey(moveBack) && currentSpeed >= 0)
        {
            currentSpeed -= brakingFactor * Time.deltaTime;
        }

        // Rotation
        if (currentSpeed != 0)
        {
            float adjustedSteering = 0f;

            if (!isBraking)
            {
                adjustedSteering = steeringSensitivity * (0.25f + currentSpeed / maxSpeed);
            }
            else if(isBraking)
            {
                adjustedSteering = steeringSensitivity * (0.75f + currentSpeed / maxSpeed);
            }
            

            if (Input.GetKey(moveLeft))
            {
                transform.Rotate(Vector3.forward, adjustedSteering * Time.deltaTime);
            }
            else if (Input.GetKey(moveRight))
            {
                transform.Rotate(Vector3.forward, -adjustedSteering * Time.deltaTime);
            }
            else if(Input.GetKey(moveLeft) && isBraking)
            {
                transform.Rotate(Vector3.forward, adjustedSteering * Time.deltaTime);
            }
            else if (Input.GetKey(moveRight) && isBraking)
            {
                transform.Rotate(Vector3.forward, -adjustedSteering * Time.deltaTime);
            }
        }
    }

    void Braking()
    {
        if (rb == null)
        {
            Debug.LogWarning("There is no Rigidbody2D on this vehicle!");
            return;
        }
        isBraking = Input.GetKey(handbreakKey);

        if (isBraking)
        {
            dragCoefficient = 2.6f;
            brakingFactor = 10f;
            currentSpeed -= brakingFactor * Time.deltaTime;
        }
        else
        {
            dragCoefficient = 0.08f;
            brakingFactor = 0.5f;
        }
    }
}
