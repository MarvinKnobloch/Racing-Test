using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Controls controls;
    private InputAction moveHotkey;

    private Rigidbody rb;
    [SerializeField] private Vector3 moveDirection;

    [SerializeField] float maxspeed;
    [SerializeField] float acceleration;
    [SerializeField] float maxacceleration;
    [SerializeField] float turningSpeed;

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
                Acceleration();
                Steering();
                break;
        }
    }
    void Update()
    {
        readMovementInput();
        switch (state)
        {
            case States.normalMovement:
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
            rb.drag = 0.5f;// + rb.velocity.sqrMagnitude / 100;
            //rb.drag = rb.velocity;
            //rb.drag = Mathf.Lerp(rb.drag, 3, Time.fixedDeltaTime * 3);
        }
        else
        {
            //rb.drag = acceleration + rb.velocity.sqrMagnitude / 1000;
            rb.drag = Mathf.Lerp(rb.drag, maxacceleration, Time.fixedDeltaTime * acceleration);

            speed = Mathf.Sign(moveDirection.y) * ((1 + rb.velocity.sqrMagnitude) / 10);

            if (speed > maxspeed) speed = maxspeed;

            if (moveDirection.y < 0) speed /= 2f;
            
            rb.AddForce(transform.forward * (speed + 20), ForceMode.Force);
        }

        //int direction = 1;
        //if (controls.Player.Accelerate.IsPressed())
        //{
        //    rb.AddRelativeForce(new Vector3(Vector3.forward.x, 0, Vector3.forward.z) * (direction* speed));
        //    rb.drag = Mathf.Lerp(rb.drag, 3, Time.fixedDeltaTime * 3);
        //}
        //else if (controls.Player.Break.IsPressed())
        //{
        //    rb.AddRelativeForce(new Vector3(Vector3.forward.x, 0, Vector3.forward.z) * (-direction * speed));
        //    rb.drag = Mathf.Lerp(rb.drag, 3, Time.fixedDeltaTime * 3);
        //}
        //else rb.drag = 0.1f + rb.velocity.sqrMagnitude / 100;


        //float force = speed * Mathf.Sign(psm.moveDirection.y);
        //if (moveDirection.y < 0) force /= 2f;

        //rig.AddForce(psm.transform.up * force, ForceMode2D.Force);

        //Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        //localVelocity.x = 0;
        //rb.velocity = transform.TransformDirection(localVelocity);
    }
    private void Steering()
    {
        rb.AddTorque(moveDirection.x * turningSpeed * Vector3.up);
    }
}
