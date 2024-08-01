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

    [SerializeField] float acceleration;
    [SerializeField] float maxspeed;

    [SerializeField] float turningSpeed;
    [SerializeField] [Range(-1, 10f)] private float driftfactor;

    [SerializeField] private float velocity;
    private Quaternion playerRotation;
    [SerializeField] private float rotationTest;
    private float angleref;
    private readonly WaitForFixedUpdate _waitForFixedUpdate = new();

    [Space]
    [SerializeField] float speed;

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
                //Steering();
                break;
        }
    }
    void Update()
    {
        readMovementInput();
        switch (state)
        {
            case States.normalMovement:
                //Acceleration();
                //KillLateralVelocity();
                //Steering();
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
            yield return _waitForFixedUpdate;
            Steering();
        }
    }
    private void Acceleration()
    {
        if (moveDirection.y == 0)
        {
            speed = Mathf.Lerp(speed, 0, 0.4f * Time.fixedDeltaTime);
        }
        else
        {
            if (moveDirection.y > 0)
            {
                if (rb.velocity.sqrMagnitude < 0.1f && speed < -1) speed = 1;
                speed = Mathf.Lerp(speed, maxspeed, Time.fixedDeltaTime * acceleration);
            }

            else if (moveDirection.y < 0)
            {
                if (rb.velocity.sqrMagnitude < 0.1f && speed > 1) speed = -1;
                speed = Mathf.Lerp(speed, -maxspeed * 0.5f, Time.fixedDeltaTime * (acceleration * 2));

            }
            //if (speed > maxspeed) speed = maxspeed;
            rb.AddForce(transform.forward * speed, ForceMode.Force);
        }
    }
    void KillLateralVelocity()
    {
        if (moveDirection.y > 0) rb.velocity = Vector3.Lerp(rb.velocity.normalized, transform.forward, driftfactor * Time.fixedDeltaTime) * rb.velocity.magnitude;
        else if (moveDirection.y < 0) rb.velocity = Vector3.Lerp(rb.velocity.normalized, -transform.forward, driftfactor * Time.fixedDeltaTime) * rb.velocity.magnitude;

        //Vector3 forwardVelocity = transform.forward * speed;
        //Vector3 lateralVelocity = transform.right * Vector3.Dot(rb.velocity, transform.right);
        //rb.velocity = forwardVelocity + lateralVelocity * driftfactor;

        velocity = rb.velocity.magnitude;
    }

    private void Steering()
    {
        float rotation = 0;
        //if (speed > 1 || speed < -1)
        {

            if (moveDirection.x > 0) rotation = turningSpeed;
            else if (moveDirection.x < 0) rotation = -turningSpeed;

            //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, transform.eulerAngles.y + rotation, ref angleref, 0.1f);
            //transform.rotation = Quaternion.Euler(0, angle, 0);

            //Vector3 newRotation = new Vector3(0, rotation, 0);
            //Quaternion deltaRotation = Quaternion.Euler(newRotation * Time.fixedDeltaTime);
            //rb.MoveRotation(rb.rotation * deltaRotation);

            playerRotation = Quaternion.Euler(0, transform.eulerAngles.y + rotation, 0);
            rb.MoveRotation(playerRotation);

            //transform.Rotate(Vector3.up * rotation);

            //transform.rotation = Quaternion.RotateTowards(transform.rotation, playerRotation, rotationTest * Time.deltaTime);

            //transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, rotationTest * Time.fixedDeltaTime);
        }
    }

        //float rotation = 0;
        ////if (speed > 1 || speed < -1)
        //{

        //    if (moveDirection.x > 0) rotation = turningSpeed;
        //    else if (moveDirection.x < 0) rotation = -turningSpeed;

        //    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, transform.eulerAngles.y + rotation, ref angleref, 0.1f);
        //    transform.rotation = Quaternion.Euler(0, angle, 0);

        //    playerRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotation, transform.eulerAngles.z);

        //    //rb.MoveRotation(playerRotation);

        //    //transform.Rotate(Vector3.up * rotation);

        //    //transform.rotation = Quaternion.RotateTowards(transform.rotation, playerRotation, rotationTest * Time.deltaTime);

        //    transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, rotationTest * Time.fixedDeltaTime);
        //}
}
