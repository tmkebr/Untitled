using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{

    private Animator anim;
    public Vector2 walkSpeed = new Vector2(2,2);
    public Vector2 sprintSpeed = new Vector2(4, 4);
    public GameObject dustEffectLeft;
    public GameObject dustEffectRight;
    public GameObject dustEffectUp;
    public GameObject dustEffectDown;
    public float dustFrequency = 0.3f;
    float dustTimer;
    //public KeyCode sprintKey = KeyCode.LeftShift;

    [HideInInspector]
    public Vector2 playerInput;

    float lastinputX, lastinputY = 0;

    //bool facingRight;

    void Start()
    {
        anim = GetComponent<Animator>(); // stores object's animations

        // set up the dust cloud timer
        dustTimer = dustFrequency;

        //facingRight = true;
    }

    void Update()
    {
        // stores the key press
        playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Debug.Log("Moving " + "X = " + playerInput.x + " Y = " + playerInput.y);

        anim.SetFloat("SpeedX", playerInput.x);
        anim.SetFloat("SpeedY", playerInput.y);

        move();        
    }

    // MOVE
    // Moves the character based on player input and speed values set in inspector
    //
    public void move()
    {
        Vector2 moveSpeed = walkSpeed;

        float curX = playerInput.x;
        float curY = playerInput.y;
        if (Mathf.Abs(curX) > 0.5f || Mathf.Abs(curY) > 0.5f)
        {
            moveSpeed = sprintSpeed;
            GameObject cloud;


            // spawn a dust cloud in the appropriate direction if we are able to
            // else count down the frequency timer
            if(dustTimer <= 0)
            {
                // reset the timer
                dustTimer = dustFrequency;

                // if running right
                if (curX > 0)
                {

                    // if also running up
                    if (curY > .5f)
                    {
                        cloud = Instantiate(dustEffectUp);
                        cloud.transform.position = new Vector2(transform.position.x + 0.5f, transform.position.y - 0.5f);
                    }

                    // if also running down
                    else if (curY < -0.5f)
                    {
                        cloud = Instantiate(dustEffectDown);
                        cloud.transform.position = new Vector2(transform.position.x + 0.5f, transform.position.y - 0.5f);
                    }

                    // else just running right
                    else
                    {
                        cloud = Instantiate(dustEffectRight);
                        cloud.transform.position = new Vector2(transform.position.x, transform.position.y - 0.5f);
                    }
                }

                // if running left
                else if (curX < 0)
                {

                    // if also running up
                    if (curY > .5f)
                    {
                        cloud = Instantiate(dustEffectUp);
                        cloud.transform.position = new Vector2(transform.position.x + 0.5f, transform.position.y - 0.5f);
                    }

                    // if also running down
                    else if (curY < -0.5f)
                    {
                        cloud = Instantiate(dustEffectDown);
                        cloud.transform.position = new Vector2(transform.position.x + 0.5f, transform.position.y - 0.5f);
                    }

                    // else just running left
                    else
                    {
                        cloud = Instantiate(dustEffectLeft);
                        cloud.transform.position = new Vector2(transform.position.x, transform.position.y - 0.5f);
                    }

                }

                else
                {
                    // if also running up
                    if (curY > .5f)
                    {
                        cloud = Instantiate(dustEffectUp);
                        cloud.transform.position = new Vector2(transform.position.x + 0.5f, transform.position.y - 0.5f);
                    }

                    // if also running down
                    else if (curY < -0.5f)
                    {
                        cloud = Instantiate(dustEffectDown);
                        cloud.transform.position = new Vector2(transform.position.x + 0.5f, transform.position.y - 0.5f);
                    }
                }
            }
            // else countdown
            else
            {
                dustTimer -= Time.deltaTime;
            }
            
        }

        // Move the character
        Vector3 movement = new Vector3(moveSpeed.x * playerInput.x, moveSpeed.y * playerInput.y, 0);
        movement *= Time.deltaTime;
        transform.Translate(movement);
    }

    void FixedUpdate()
    {
        lastinputX = playerInput.x;
        lastinputY = playerInput.y;

        if (lastinputX != 0 || lastinputY != 0)
        {
            anim.SetBool("walking", true);


            if (lastinputX > 0)
            {
                anim.SetFloat("LastMoveX", 1f);
            }
            else if (lastinputX < 0)
            {
                anim.SetFloat("LastMoveX", -1f);
            }
            else
            {
                anim.SetFloat("LastMoveX", 0f);
            }


            if (lastinputY > 0)
            {
                anim.SetFloat("LastMoveY", 1f);
            }
            else if (lastinputY < 0)
            {
                anim.SetFloat("LastMoveY", -1f);

            }
            else
            {
                anim.SetFloat("LastMoveY", 0f);
            }

        }
        else
        {
            anim.SetBool("walking", false);
        }

    }

    // flips the object. Called when object is facing direction opposite of its current input
    //private void Flip()
    //{

    //    facingRight = !facingRight;

    //    // Multiply the player's x local scale by -1.
    //    Vector3 theScale = transform.localScale;
    //    theScale.x *= -1;
    //    transform.localScale = theScale;
    //}

    // CHECK TURN
    // Manages flipping of the character
    // Flips the character if the input is in the opposite direction they're facing
    // DEPRECATED DUE TO BLEND TREE
    /*public void checkTurn()
    {

        if (playerInput.x < 0)
        {
            facingRight = false;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else if (playerInput.x > 0)
        {
            facingRight = true;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        //// If the player is currently facing right, and has input left
        //if (facingRight && playerInput.x == -1)
        //{
        //    // flip the character
        //    Flip();
        //}
        //else if (!facingRight && playerInput.x == 1)
        //{
        //    // else if the character is facing left and is going right, flip the character
        //    Flip();
        //}
    }*/


}
