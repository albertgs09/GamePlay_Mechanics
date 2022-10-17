using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;


    public float dashSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce, jumpCoolDown, airMultiplier;
    public bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask Ground;
    public bool grounded;

    public Transform orientation;
    public Animator playerAC;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        dashing,
        air
    }

    public bool dashing;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;//otherwise player falls over
    }


    // Update is called once per frame
    void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, Ground);

        MyInput();
        SpeedControl();

        //handle drag
        if (state != MovementState.dashing)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        var movement = new Vector3(horizontalInput, 0f, verticalInput);
        if(movement.magnitude > 0)
        {
            playerAC.SetBool("IsWalking", true);
        }
        else
        {
            playerAC.SetBool("IsWalking", false);
        }

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            //so you will be able to continuosly jump if you hold down the jump key
            Invoke(nameof(ResetJump), jumpCoolDown);
        }


        //Attacks
        if (Input.GetMouseButtonDown(0))
        {
            playerAC.SetTrigger("Attack");
        }
    }

    private void StateHandler()
    {
        //Mode - Dashing
        if (dashing)
        {
            state = MovementState.dashing;
            moveSpeed = dashSpeed;
        }
    }

    private void MovePlayer()
    {
        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //on ground
        if(grounded)
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);//adds force to forward direction

        //in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);//adds force to forward direction when your in the air

    }
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
      
    }

    void Jump()
    {
        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);// so it can jump the same height

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);// use impulse cause you're only applying the force once
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
