using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Controls controls;
    private InputAction moveHotkey;

    private Rigidbody rb;
    private Vector3 moveDirection;

    [SerializeField] float acceleration;
    [SerializeField] float maxspeed;

    [SerializeField] float turningSpeed;
    [SerializeField] [Range(0, 10f)] private float driftfactor;

    [SerializeField] private float velocity;
    private Quaternion playerRotation;

    [Space]
    [SerializeField] float speed;

    public States state;

    public enum States
    {
        normalMovement,
    }
    private void Awake()
    {
        controls = new Controls();
        moveHotkey = controls.Player.Move;

        rb = GetComponent<Rigidbody>();
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
                //Acceleration();
                //KillLateralVelocity();
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
                Acceleration();
                KillLateralVelocity();
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
    private void Acceleration()
    {
        if (moveDirection.y == 0)
        {
            speed = Mathf.Lerp(speed, 0, Time.fixedDeltaTime);
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

            playerRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotation, transform.eulerAngles.z);
            //rb.MoveRotation(playerRotation);
            // transform.Rotate(Vector3.up * rotation);
        }
    }
    private void LateUpdate()
    {
        float rotation = 0;
        //if (speed > 1 || speed < -1)
        {
            if (moveDirection.x > 0) rotation = turningSpeed;
            else if (moveDirection.x < 0) rotation = -turningSpeed;

            playerRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotation, transform.eulerAngles.z);
            // transform.Rotate(Vector3.up * rotation);
            rb.MoveRotation(playerRotation);
        }
    }
}
