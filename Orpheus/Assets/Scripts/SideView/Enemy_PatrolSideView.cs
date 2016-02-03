using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy_PatrolSideView : RaycastController
{

    public LayerMask passengerMask;

    [Tooltip("Add in patrol points")]
    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    [Tooltip("Speed of the enemy relative to the player (player speed = 1")]
    public float speed;
    [Tooltip("Should the enemy rotate?")]
    public bool cyclic;
    [Tooltip("How long should the enemy wait?")]
    public float waitTime;
    [Tooltip("Higher easing means more time spent at the endpoints and a quicker move through the middle")]
    [Range(0, 2)]
    public float easeAmount;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;

    public bool facingRight = true;
    public bool frozen;

    // Animation Variables
    Animator anim; // the main animator of the enemy

    //List<PassengerMovement> passengerMovement;
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start()
    {

        //INITIALIZATION
        base.Start(); // start calculating rays
        anim = GetComponent<Animator>(); // stores enemy's animations
        globalWaypoints = new Vector3[localWaypoints.Length]; // store the waypoints
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

        // calculate the velocity of the enemy and its passenger only if its not frozen
        if (!frozen)
        {
            Vector3 velocity = CalculateMovement();
            //CalculatePassengerMovement(velocity);


            // move the player if colliding
            //MovePassengers(true);
            transform.Translate(velocity);
            //MovePassengers(false);
        }
        
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

        //print("New Position = " + newPos);


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

            // the enemt is now facing left
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

    public void stop()
    {
        anim.SetBool("isStopped", true);
        frozen = true;
    }

    //// MOVE PASSENGERS
    //// Moves passangers atop the enemy
    //// also properly pushes the player by the enemy
    //// param: boolean whether the platform (enemy) has begun moving or not
    //void MovePassengers(bool beforeMovePlatform)
    //{
    //    foreach (PassengerMovement passenger in passengerMovement)
    //    {
    //        if (!passengerDictionary.ContainsKey(passenger.transform))
    //        {
    //            passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
    //        }

    //        if (passenger.moveBeforePlatform == beforeMovePlatform)
    //        {
    //            passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
    //        }
    //    }
    //} 

    //// CALCULATE PASSENGER MOVEMENT
    //// calculates how much the passenger atop the enemy should move
    //// also calculates how much the enemt should push the player
    //// param: velocity vector of the enemy
    //void CalculatePassengerMovement(Vector3 velocity)
    //{
    //    HashSet<Transform> movedPassengers = new HashSet<Transform>();
    //    passengerMovement = new List<PassengerMovement>();

    //    float directionX = Mathf.Sign(velocity.x);
    //    float directionY = Mathf.Sign(velocity.y);

    //    // Vertically moving platform
    //    if (velocity.y != 0)
    //    {
    //        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

    //        for (int i = 0; i < verticalRayCount; i++)
    //        {
    //            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
    //            rayOrigin += Vector2.right * (verticalRaySpacing * i);
    //            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

    //            if (hit && hit.distance != 0)
    //            {
    //                if (!movedPassengers.Contains(hit.transform))
    //                {
    //                    movedPassengers.Add(hit.transform);
    //                    float pushX = (directionY == 1) ? velocity.x : 0;
    //                    float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

    //                    passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
    //                }
    //            }
    //        }
    //    } 

    //    // Horizontally moving platform
    //    if (velocity.x != 0)
    //    {
    //        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

    //        for (int i = 0; i < horizontalRayCount; i++)
    //        {
    //            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
    //            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
    //            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

    //            if (hit && hit.distance != 0)
    //            {
    //                if (!movedPassengers.Contains(hit.transform))
    //                {
    //                    movedPassengers.Add(hit.transform);
    //                    float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
    //                    float pushY = -skinWidth;

    //                    passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
    //                }
    //            }
    //        }
    //    }

    //    // Passenger on top of a horizontally or downward moving platform
    //    if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
    //    {
    //        float rayLength = skinWidth * 2;

    //        for (int i = 0; i < verticalRayCount; i++)
    //        {
    //            Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
    //            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

    //            if (hit && hit.distance != 0)
    //            {
    //                if (!movedPassengers.Contains(hit.transform))
    //                {
    //                    movedPassengers.Add(hit.transform);
    //                    float pushX = velocity.x;
    //                    float pushY = velocity.y;

    //                    passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
    //                }
    //            }
    //        }
    //    }
    //}

    //struct PassengerMovement
    //{
    //    public Transform transform;
    //    public Vector3 velocity;
    //    public bool standingOnPlatform;
    //    public bool moveBeforePlatform;

    //    public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
    //    {
    //        transform = _transform;
    //        velocity = _velocity;
    //        standingOnPlatform = _standingOnPlatform;
    //        moveBeforePlatform = _moveBeforePlatform;
    //    }
    //}

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
