using UnityEngine;
using System.Collections;

public class CharacterControllerSideways : MonoBehaviour
{

    private Animator anim;
    public Vector2 walkSpeed = new Vector2(2, 2);
    public Vector2 sprintSpeed = new Vector2(4, 4);
    public KeyCode sprintKey = KeyCode.LeftShift;

    [HideInInspector]
    public Vector2 playerInput;

    float lastinputX, lastinputY = 0;

    //bool facingRight;

    void Start()
    {
        anim = GetComponent<Animator>(); // stores object's animations

        //facingRight = true;
    }

    void Update()
    {
        // stores the key press
        playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

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
        if (Input.GetKey(sprintKey))
        {
            moveSpeed = sprintSpeed;
        }

        // Move the character
        Vector3 movement = new Vector3(moveSpeed.x * playerInput.x, 0f, 0f);
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
}
