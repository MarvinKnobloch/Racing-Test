using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [NonSerialized] public Controls controls;
    private InputAction moveHotkey;

    [NonSerialized] public Rigidbody rb;
    [NonSerialized] public Collider playerCollider;
    public Vector3 moveDirection;
    [NonSerialized] public Transform ChildTransform;
    public float baseGravity;
    public float maxGravity;
    public float rotationSpeed;

    [NonSerialized] public float airIntoGroundTime;

    [Header("SpeedValues")]
    public float acceleration;

    public float speed;
    public float maxspeed;
    [Range(0.1f, 4)] public float reduceSpeedIfNoInput;
    [Range(0.1f, 4)] public float boostSpeedReduction;
    [Range(0, 1)] public float keepSpeedOnCollision;

    public float bonusSpeed;
    public float maxbonusSpeed;

    public float beyondMaxSpeed;
    [NonSerialized] public float beyondMaxSpeedTime;
    [Range(0.2f, 2)] public float beyondMaxSpeedGainInterval;
    [Range(0.2f, 2)] public float beyondMaxSpeedLoseInterval;

    public float finalSpeed;

    [Header("HandlingValues")]
    public float turningSpeed;
    public float baseDriftTurningSpeed;
    public float minDriftTurningSpeed;
    public float maxDriftTurningSpeed;
    private float baseTurningSpeed;

    [Range(0, 10f)] public float driftFactor;
    private float baseDriftFactor;

    public bool drifting;
    public bool driftingLeft;

    public float driftime;
    public float maxDrifttime;
    public bool getDriftBoost;
    public int driftBoost;


    [NonSerialized] public Quaternion playerRotation;
    [NonSerialized] public float XAirRotation;

    public float currentGroundAngle;
    [NonSerialized] public Vector3 groundVector;
    public LayerMask groundCheckLayer;


    [NonSerialized] public PlayerMovement playerMovement = new PlayerMovement();

    public float BonusSpeed
    {
        get { return bonusSpeed; }
        set { bonusSpeed = Math.Min(Math.Max(0, value), maxbonusSpeed); }
    }

    public States state;

    public enum States
    {
        groundMovement,
        airMovement,
        airIntoGround,
    }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        controls = new Controls();
        moveHotkey = controls.Player.Move;

        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<BoxCollider>();
        ChildTransform = transform.GetChild(0).transform;

        baseDriftFactor = driftFactor;
        baseTurningSpeed = turningSpeed;

        playerMovement.player = this;

        StartCoroutine(LateFixedUpdate());

    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case States.groundMovement:
                playerMovement.GroundMovement();
                playerMovement.KillLateralVelocity();
                break;
            case States.airMovement:
                playerMovement.AirMovement();
                break;
            case States.airIntoGround:
                playerMovement.AirMovement();
                break;
        }
    }
    void Update()
    {
        ReadMovementInput();
        switch (state)
        {
            case States.groundMovement:
                playerMovement.GroundCheck();
                playerMovement.Drift();
                playerMovement.BeyondMaxSpeed();
                break;
            case States.airMovement:
                playerMovement.AirCheck();
                break;
            case States.airIntoGround:
                playerMovement.GroundCheck();
                AirIntoGroundTransition();
                playerMovement.BeyondMaxSpeed();
                break;
        }
    }
    private void ReadMovementInput()
    {
        moveDirection = moveHotkey.ReadValue<Vector2>();
        if (moveDirection.x > 0) moveDirection.x = 1;
        else if (moveDirection.x < 0) moveDirection.x = -1;

        if (moveDirection.y > 0) moveDirection.y = 1;
        else if (moveDirection.y < 0) moveDirection.y = -1;
    }

    private IEnumerator LateFixedUpdate()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            switch (state)
            {
                case States.groundMovement:
                    if (!drifting) playerMovement.Steering();
                    else playerMovement.DriftSteering();
                    break;
                case States.airMovement:
                    playerMovement.AirSteering();
                    //playerMovement.AirRotation();
                    break;
                case States.airIntoGround:
                    playerMovement.AirSteering();
                    break;
            }
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("reduce speed");
        //speed *= keepSpeedOnCollision;
    }
    public IEnumerator ChangeDriftfactor()
    {
        while (driftFactor < baseDriftFactor - 0.2f)
        {
            driftFactor = Mathf.Lerp(driftFactor, baseDriftFactor, 0.3f * Time.fixedDeltaTime);
            //driftFactor += Time.deltaTime * 2;
            yield return null;
        }
        driftFactor = baseDriftFactor;
    }
    public void SwitchToGroundState()
    {
        state = States.groundMovement;
    }
    public void SwitchToAirState()
    {
        XAirRotation = 0;
        drifting = false;
        driftime = 0;
        state = States.airMovement;
    }
    public void SwitchAirIntoGround()
    {
        airIntoGroundTime = 0;
        state = States.airIntoGround;
    }
    private void AirIntoGroundTransition()
    {
        airIntoGroundTime += Time.deltaTime;
        if(airIntoGroundTime > 0.1f)
        {
            SwitchToGroundState();
        }
    }

}
