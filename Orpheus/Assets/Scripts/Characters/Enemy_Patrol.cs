using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Enemy))]
public class Enemy_Patrol : RaycastController
{

    //public LayerMask passengerMask;
    //Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    // Public Variables
    [Tooltip("Add in patrol points")]
    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;
    [Tooltip("Speed of the enemy relative to the player (player speed = 1")]
    [Range(0, 100)]
    public float speed;
    public enum enemyType { PATROLLER };
    [Tooltip("Should the enemy rotate?")]
    public bool cyclic;
    [Tooltip("How long should the enemy wait?")]
    public float waitTime;
    [Tooltip("Higher easing means more time spent at the endpoints and a quicker move through the middle")]
    [Range(0, 2)]
    public float easeAmount;
    public Status curStatus;

    // storage variables
    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;

    // movement variables
    bool facingRight = true;
    public bool frozen;

    Collider sight;
    [HideInInspector]
    public Enemy connectedEnemy;
    Animator anim; // the main animator of the enemy

    public enum Status { PATROL, ALERT };

    public override void Start()
    {
        // start the enemy as patrolling
        curStatus = Status.PATROL;

        // get the associated collider
        sight = GetComponent<CapsuleCollider>();
        connectedEnemy = GetComponent<Enemy>();

        //INITIALIZATION
        base.Start(); // start calculating rays
        anim = GetComponent<Animator>(); // stores enemy's animations
        globalWaypoints = new Vector3[localWaypoints.Length]; // store the waypoints

        // fill the array
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    // UPDATE
    // called once per frame
    void Update()
    {

        UpdateRaycastOrigins();

        // PATROL
        if (curStatus == Status.PATROL && !frozen)
        {
            patrol();
        }

        // ALERT
        else if (curStatus == Status.ALERT)
        {
            // move to location of player
            // sniff animation
        }
    }

    // PATROL
    // Calculates the movement between the patrol points and moves the enemy
    void patrol()
    {
        // calculate and move the enemy between its patrol points
        Vector3 velocity = CalculateMovement();
        transform.Translate(velocity);
    }

    /// <summary>
    /// STOP:
    /// stops the Patrol and freezes and kills the enemy
    /// </summary>
    public void stop()
    {
        // stop the animator
        anim.SetBool("isStopped", true);

        // freeze the enemy
        frozen = true;

        // kill the enemy
        connectedEnemy.killEnemy();
    }

    // EASE
    // eases the enemy movement between the two values
    // Higher easing means more time spent at the endpoints and a quicker move through the middle
    // param
    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    // CALCULATE MOVEMENT
    // Calculates how much the enemy should move for the next frame
    // takes into account flipping, waiting, and cycling
    // return: the new position of the enemy at the next frame
    Vector3 CalculateMovement()
    {

        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        anim.SetBool("isStopped", false);
        //print("Moving");

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;

        // calculate distance between waypoints by calling the distance function
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);


        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);


        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        // if at the end of the cycle
        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
            // bool sniff true

            // The enemy is stopped. Trigger animations
            anim.SetBool("isStopped", true);
            //print("Stopped");

        }

        // flips the enemy if needed
        flip(newPos, transform.position);

        // return the new place to move and store it
        return newPos - transform.position;
    }


    // FLIP
    // flips the object based on two positions' x values
    // param: the two positions
    void flip(Vector3 newPosition, Vector3 oldPosition)
    {
        Vector3 theScale = transform.localScale;

        if (newPosition.x < oldPosition.x && facingRight)
        {
            // the enemy is now facing left
            facingRight = false;


            theScale.x *= -1;
            transform.localScale = theScale;
        }
        else if (newPosition.x > oldPosition.x && !facingRight)
        {
            // the enemy is now facing right
            facingRight = true;

            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
 

    // Draws the patrol points crosshairs
    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }

}
