using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{

    private Animator anim;
    public Vector2 walkSpeed = new Vector2(1,1);
    public Vector2 sprintSpeed = new Vector2(2, 2);
    public KeyCode sprintKey = KeyCode.LeftShift;

    [HideInInspector]
    public Vector2 playerInput;

    float lastinputX, lastinputY = 0;

    bool facingRight;

    void Start()
    {
        anim = GetComponent<Animator>(); // stores object's animations

        facingRight = true;
    }

    void Update()
    {
        // stores the key press
        playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        anim.SetFloat("SpeedX", playerInput.x);
        anim.SetFloat("SpeedY", playerInput.y);

        checkTurn();
        move();        
    }

    // CHECK TURN
    // Manages flipping of the character
    // Flips the character if the input is in the opposite direction they're facing
    public void checkTurn()
    {
        // If the player is currently facing right, and has input left
        if (facingRight && playerInput.x == -1)
        {
            // flip the character
            Flip();
        }
        else if (!facingRight && playerInput.x == 1)
        {
            // else if the character is facing left and is going right, flip the character
            Flip();
        }
    }

    // MOVE
    // Moves the character based on player input and speed values set in inspector
    //
    public void move()
    {
        Vector2 moveSpeed = walkSpeed;
        if (Input.GetKey(sprintKey))
        {
            moveSpeed = sprintSpeed;
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
    private void Flip()
    {

        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}