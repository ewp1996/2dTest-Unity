using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


//NOTES
//Use object pooling for the after images on the dash
//if you instantiate and destroy after image sprites the normal way it will take up A LOT of resources and will slow down the game
//So we will create a bunch of gameobjects at the start of the game and leave them in a "pool" off to the side, and grab them when we need them.
//So when sprite is needed its grabbed from the pool, used, and then sent back to the pool
//this means we only create the objects once and destroy them once

public class PlayerController : MonoBehaviour
{
    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    private float dashTimeLeft;
    //last x coord where we place afterimage
    private float lastImageXpos;
    //keeps track of last time we started a dash and will be used to check for the cooldown
    private float lastDash = -100f;

    private int amountOfJumpsLeft;
    //-1 = left, 1 = right
    //needed as an int for wall jumping
    private int facingDirection = 1;
    private int lastWallJumpDirection;

    [SerializeField] private bool isFacingRight = true;
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool canNormalJump;
    [SerializeField] private bool canWallJump;
    [SerializeField] private bool isTouchingWall;
    [SerializeField] private bool isWallSliding;
    [SerializeField] private bool isAttemptingJump;
    //checks to see if you have let go of spacebar, and if you have it adds the jump multiplier
    [SerializeField] private bool checkJumpMultiplier;
    [SerializeField] private bool canMove;
    [SerializeField] private bool canFlip;
    [SerializeField] private bool hasWallJumped;
    [SerializeField] private bool isDashing;


    private Rigidbody2D rb;
    private Animator anim;

    public int amountOfJumps = 2;
    
    
    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    //if you jump AND THEN move, this determines how far you can get
    public float movementForceInAir;
    //used to help give a floaty fall after no movement input. DO NOT MAKE HIGHER THAN 1
    public float airDragMultiplier = 0.95f;
    //Like how in many games if you hold space or tap, jump height is different
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce;
    public float wallJumpForce;
    //time after jumping or falling that you can jump again
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    public float wallJumpTimerSet = 0.5f;
    public float dashTime;
    public float dashSpeed;
    public float distanceBetweenImages;
    public float dashCooldown;

    //vector that determines direction that we jump off of walls and change the angle that we hop off wall
    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;

    public LayerMask whatIsGround;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
        
        //makes vectors equal 1
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
        CheckDash();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding()
    {
        //use if velocity.y < 0 so that you arent wall sliding while moving up the wall, only when coming back down
        if(isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckMovementDirection()
    {
        if(isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if(!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }


        //there are apparently transitions problems that can happen because of this if statement.
        //I believe this is the correct statement but it could be (Mathf.Abs(rb.velocity.x) >= 0.01f)
        if (rb.velocity.x > 0.01f || rb.velocity.x < -0.01f)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            //add isTouchingWall so that it knows you want to wall jump and not use your double jump instead
            if(isGrounded || (amountOfJumpsLeft > 0 && !isWallSliding))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingJump = true;
            }
        }

        if(Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if(!isGrounded && movementInputDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if(isTouchingWall && isGrounded)
        {
            isWallSliding = false;
        }

        if (!canMove)
        {
            turnTimer -= Time.deltaTime;

            if(turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        if (checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }

        if (Input.GetButtonDown("Dash") && isWalking)
        {
            if(Time.time >= (lastDash + dashCooldown))
            {
                AttemptToDash();
            }
        }
    }

    private void AttemptToDash()
    {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXpos = transform.position.x;
    }

    private void CheckDash()
    {
        if (isDashing)
        {
            if(dashTimeLeft > 0)
            {
                canMove = false;
                canFlip = false;
                //IMPORTANT MECHANIC if you dont want character to fall while dashing, set y to 0
                rb.velocity = new Vector2(dashSpeed * facingDirection, rb.velocity.y);
                dashTimeLeft -= Time.deltaTime;

                //check if enough time has happened to place a new afterimage
                if (Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXpos = transform.position.x;
                }
            }

            if(dashTimeLeft <= 0 || isTouchingWall)
            {
                isDashing = false;
                canMove = true;
                canFlip = true;
            }
        }
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y <= 0.01f)
        {
            amountOfJumpsLeft = amountOfJumps;
        }

        if(isTouchingWall)
        {
            canWallJump = true;
        }
        
        if(amountOfJumpsLeft <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }
    }

   /* //Splitting it into two seperate functions below
    private void Jump()
    {
        if (canJump && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
        }
        //wall hop
        //don't know if really needed right now
        else if (isWallSliding && movementInputDirection == 0 && canJump) 
        {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            //a small push(Impulse) in that direction with the power of forceToAdd above
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
        else if ((isWallSliding || isTouchingWall) && movementInputDirection != 0 && canJump) //wall jump
        {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            //a small push(Impulse) in that direction with the power of forceToAdd above
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
    }
   */

    private void CheckJump()
    {
        if(jumpTimer > 0)
        {
            //wall jump
            if(!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection != facingDirection)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }
        }
        
        if(isAttemptingJump)
        {
            jumpTimer -= Time.deltaTime;
        }

        //this if statement uses wallJumpTimer to prevent player from wall jumping up one wall by cutting off the y velocity if the player turns back after wall jumping
        if(wallJumpTimer > 0)
        {
            if(hasWallJumped && movementInputDirection == -lastWallJumpDirection)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            } 
            else if(wallJumpTimer <= 0)
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime; 
            }
        }
    }

    private void NormalJump()
    {
        if (canNormalJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
            jumpTimer = 0;
            isAttemptingJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        if (canWallJump) //wall jump
        {
            //have to set y velocity because if falling rapidly and jump, the force will move you up very much
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            //this line makes it so that when you jump from one wall to another it doesnt take up a jump. AKA INFINITE WALL JUMPS
            amountOfJumpsLeft = amountOfJumps;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            //a small push(Impulse) in that direction with the power of forceToAdd above
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingJump = false;
            checkJumpMultiplier = true;
            //these 3 lines below stop player from sticking to wall when trying to jump to next wall
            turnTimer = 0;
            canMove = true;
            canFlip = true;

            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void ApplyMovement()
    {
        //only changes x velocity when grounded
        //important for chaining wall jumps and not falling right after leaving a wall

        //gives more of a floaty feeling when falling after no movement input
        if (!isGrounded && !isWallSliding && movementInputDirection == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        else if(canMove)
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }
       
        
        //this is so you slowly move down when next to the wall
        if (isWallSliding)
        {
            if(rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    private void Flip()
    {
        //putting these lines within this if statement fixes the issue of falling when immediately leaving a wall
        if (!isWallSliding && canFlip)
        {
            //fliping for wall jumping
            facingDirection *= -1;
            
            //flipping for walking, jumping, etc.
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }


    //gizmos are visual cues that help to debug
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
