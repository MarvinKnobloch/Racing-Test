using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    private Controls controls;
    private InputAction moveHotkey;

    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("SpeedValues")]
    [SerializeField] private float acceleration;
    [SerializeField] private float maxspeed;
    [SerializeField] [Range(0.1f, 4)] private float reduceSpeedIfNoInput;
    [SerializeField] [Range(0, 1)] private float keepSpeedOnCollision;

    [Header("HandlingValues")]
    [SerializeField] private float turningSpeed;
    [SerializeField] private float baseDriftTurningSpeed;
    [SerializeField] private float minDriftTurningSpeed;
    [SerializeField] private float maxDriftTurningSpeed;
    private float baseTurningSpeed;

    [SerializeField] [Range(0, 10f)] private float driftFactor;
    private float baseDriftFactor;

    [SerializeField] private bool drifting;
    [SerializeField] private bool driftingLeft;

    [SerializeField] private float driftime;
    [SerializeField] private float maxDrifttime;
    [SerializeField] private bool getDriftBoost;
    [SerializeField] private int driftBoost;

    [Space]
    [SerializeField] float speed;

    private Quaternion playerRotation;

    [SerializeField] private bool isGrounded;
    [SerializeField] private float currentGroundAngle;
    private Vector3 groundVector;
    [SerializeField] private LayerMask groundCheckLayer;


    public States state;

    public enum States
    {
        normalMovement,
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

        baseDriftFactor = driftFactor;
        baseTurningSpeed = turningSpeed;

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
            case States.normalMovement:
                Acceleration();
                KillLateralVelocity();
                break;
        }
    }
    void Update()
    {
        readMovementInput();
        switch (state)
        {
            case States.normalMovement:
                GroundCheck();
                Drift();
                break;
        }
    }

    private void readMovementInput()
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
            if (!drifting) Steering();
            else DriftSteering();
        }
    }
    private void Acceleration()
    {
        if (moveDirection.y == 0)
        {
            //Mathf.Lerp(speed, 0, reduceSpeed * Time.fixedDeltaTime);
            //rb.drag = reduceSpeed;
            speed = Mathf.Lerp(speed, 0, reduceSpeedIfNoInput * Time.fixedDeltaTime);
        }
        else
        {
            if (moveDirection.y > 0)
            {
                if (rb.velocity.sqrMagnitude < 0.1f && speed < 1) speed = 1;
                speed = Mathf.Lerp(speed, maxspeed, Time.fixedDeltaTime * acceleration);
            }

            else if (moveDirection.y < 0)
            {
                if (rb.velocity.sqrMagnitude < 0.1f && speed > 1) speed = -1;
                speed = Mathf.Lerp(speed, -maxspeed * 0.5f, Time.fixedDeltaTime * (acceleration * 2));

            }
        }
        if (GroundCheck())
        {
            isGrounded = true;
            rb.AddForce(Vector3.ProjectOnPlane(transform.forward, groundVector).normalized * speed, ForceMode.Force);
            rb.AddForce(-transform.up * 3, ForceMode.Force);
        }
        else
        {
            isGrounded = false;
            rb.AddForce(transform.forward * speed, ForceMode.Force);
            rb.AddForce(-Vector3.up * 10, ForceMode.Force);
        }

    }
    void KillLateralVelocity()
    {
        if (moveDirection.y > 0) rb.velocity = Vector3.Lerp(rb.velocity.normalized, transform.forward, driftFactor * Time.fixedDeltaTime) * rb.velocity.magnitude;
        else if (moveDirection.y < 0) rb.velocity = Vector3.Lerp(rb.velocity.normalized, -transform.forward, driftFactor * Time.fixedDeltaTime) * rb.velocity.magnitude;

        //Vector3 forwardVelocity = transform.forward * speed;
        //Vector3 lateralVelocity = transform.right * Vector3.Dot(rb.velocity, transform.right);
        //rb.velocity = forwardVelocity + lateralVelocity * driftfactor;
    }

    private void Steering()
    {
        float rotation = 0;
        //if (speed > 1 || speed < -1)
        {

            if (moveDirection.x > 0) rotation = turningSpeed;
            else if (moveDirection.x < 0) rotation = -turningSpeed;

            playerRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotation, transform.eulerAngles.z);
            rb.MoveRotation(playerRotation);

            //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, transform.eulerAngles.y + rotation, ref angleref, 0.1f);
            //transform.rotation = Quaternion.Euler(0, angle, 0);

            //Vector3 newRotation = new Vector3(0, rotation, 0);
            //Quaternion deltaRotation = Quaternion.Euler(newRotation * Time.fixedDeltaTime);
            //rb.MoveRotation(rb.rotation * deltaRotation);


            //transform.Rotate(Vector3.up * rotation);

            //transform.rotation = Quaternion.RotateTowards(transform.rotation, playerRotation, rotationTest * Time.deltaTime);

            //transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, rotationTest * Time.fixedDeltaTime);
        }
    }
    private void DriftSteering()
    {
        float rotation;
        if (driftingLeft)
        {
            if (moveDirection.x > 0) rotation = maxDriftTurningSpeed;
            else if (moveDirection.x < 0) rotation = minDriftTurningSpeed;
            else rotation = baseDriftTurningSpeed;
        }
        else
        {
            if (moveDirection.x > 0) rotation = minDriftTurningSpeed;
            else if (moveDirection.x < 0) rotation = maxDriftTurningSpeed;
            else rotation = baseDriftTurningSpeed;

            rotation *= -1;
        }

        playerRotation = Quaternion.Euler(0, transform.eulerAngles.y + rotation, 0);
        rb.MoveRotation(playerRotation);
    }
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("reduce speed");
        //speed *= keepSpeedOnCollision;
    }
    private void Drift()
    {
        if (controls.Player.Drift.WasPerformedThisFrame() && moveDirection.x != 0 && speed > 3)
        {
            if (moveDirection.x > 0) driftingLeft = true;
            else if (moveDirection.x < 0) driftingLeft = false;
            drifting = true;
            driftFactor = 0.1f;
        }

        if (controls.Player.Drift.WasReleasedThisFrame())
        {
            drifting = false;
            driftime = 0;

            if (getDriftBoost)
            {
                speed += driftBoost;
                getDriftBoost = false;
            }

            StopCoroutine(nameof(ChangeDriftfactor));
            StartCoroutine(ChangeDriftfactor());
        }

        DriftTimerUpdate();
    }
    private void DriftTimerUpdate()
    {
        if (drifting)
        {
            driftime += Time.deltaTime;
            if (driftime > maxDrifttime)
            {
                getDriftBoost = true;
            }
        }
    }
    private IEnumerator ChangeDriftfactor()
    {
        while (driftFactor < baseDriftFactor - 0.2f)
        {
            driftFactor = Mathf.Lerp(driftFactor, baseDriftFactor, 0.3f * Time.fixedDeltaTime);
            //driftFactor += Time.deltaTime * 2;
            yield return null;
        }
        driftFactor = baseDriftFactor;
    }
    private bool GroundCheck()
    {
        if (Physics.BoxCast(transform.position + transform.up * 0.3f, transform.localScale * 0.5f, -transform.up, out RaycastHit hit, transform.rotation, 5f, groundCheckLayer))
        {
            currentGroundAngle = Vector3.Angle(Vector3.up, hit.normal);
            groundVector = hit.normal;
            return true;
        }
        return false;
    }

}
