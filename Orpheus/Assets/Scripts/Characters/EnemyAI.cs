using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class EnemyAI : MonoBehaviour
{
    // What to chase?
    Vector3 curTarget;
    //public Vector3[] localWaypoints;
    public Vector3[] globalWaypoints;
    int currentWaypoint;

    // How many times each second we will update our path
    public float updateRate = 2f;

    // Caching
    private Seeker seeker;

    //The calculated path
    public Path path;

    //The AI's speed per second
    public float speed = 1f;
    public float rotationSpeed = 1f;
    public Transform sight;

    [HideInInspector]
    public bool pathIsEnded = false;

    // The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextWaypointDistance = .05f;

    int storageWaypoint;

    bool facingRight;

    private Vector3 lookDirection;
    private Quaternion lookRotation;
    private Quaternion _facing;

    void Start()
    {
        seeker = GetComponent<Seeker>();

        facingRight = true;
        _facing = sight.rotation;

        //getGlobalWaypoints();
        curTarget = globalWaypoints[0];

        if (curTarget == Vector3.zero)
        {
            Debug.LogError("No target found... Please enter a target transform.");
            return;
        }

        // Start a new path to the target position, return the result to the OnPathComplete method
        seeker.StartPath(transform.position, curTarget, OnPathComplete);

        StartCoroutine(UpdatePath());



    }

    IEnumerator UpdatePath()
    {
        if (curTarget == Vector3.zero)
        {
            //TODO: Insert a player search here.
            Debug.Log("No target found... Please enter a target transform");
            yield return false;
        }


        // Start a new path to the target position, return the result to the OnPathComplete method
        seeker.StartPath(transform.position, curTarget, OnPathComplete);

        yield return new WaitForSeconds(1f / updateRate);
        StartCoroutine(UpdatePath());
    }

    public void OnPathComplete(Path p)
    {

        // pause the enemy

        Debug.Log("We got a path. Did it have an error? " + p.error);
        if (!p.error)
        {
            path = p;

            storageWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        if (curTarget == Vector3.zero)
        {
            //TODO: Insert a player search here.
            Debug.Log("No target found... Please enter a target transform");
            return;
        }

        //TODO: Always look at player?

        if (path == null)
            return;

        if (storageWaypoint >= path.vectorPath.Count)
        {
            if (pathIsEnded)
                return;

            Debug.Log("End of path reached.");
            pathIsEnded = true;

            // get a new destination
            curTarget = chooseRandomWaypoint(globalWaypoints);     
            return;
        }
        pathIsEnded = false;

        //Direction to the next waypoint
        Vector3 dir = (path.vectorPath[storageWaypoint] - transform.position).normalized;
        dir *= speed * Time.fixedDeltaTime;

        //Move the AI and flip if needed
        flip((transform.position + dir), transform.position);
        //pivot(dir);
        //sight.rotation = Quaternion.LookRotation(Vector3.forward, dir);

        float rot_z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        sight.rotation = Quaternion.Euler(0f, 0f, rot_z);

        // move the enemy
        transform.Translate(dir);

        float dist = Vector3.Distance(transform.position, path.vectorPath[storageWaypoint]);
        if (dist < nextWaypointDistance)
        {
            storageWaypoint++;
            return;
        }
    }

    // PIVOT
    // pivots the sight collider towards the movement direction
    // param: direction of movement
    void pivot(Vector3 direction)
    {
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 180;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        sight.rotation = Quaternion.Slerp(sight.rotation, q, Time.deltaTime * rotationSpeed);
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

    //void getGlobalWaypoints()
    //{
    //    // compute global waypoints relative to the local waypoints set
    //    globalWaypoints = new Vector3[localWaypoints.Length]; // store the waypoints
    //    // fill the array
    //    for (int i = 0; i < localWaypoints.Length; i++)
    //    {
    //        globalWaypoints[i] = localWaypoints[i] + transform.position;
    //    }
    //}

    Vector3 chooseRandomWaypoint(Vector3[] wayPoints)
    {
        pathIsEnded = false;
        int chosenVal = Random.Range(0, wayPoints.Length);

        // update current waypoint and return target location
        currentWaypoint = chosenVal;
        return wayPoints[chosenVal];
    }

    Vector3 getClosestWaypoint(Vector3 curPosition, Vector3[] wayPoints)
    {
        float closestDistance = 0f;
        int closestWaypoint = 0;

        // catch errors
        if (wayPoints == null)
        {
            Debug.Log("Unable to get closest waypoint: no waypoints provided");
            return Vector3.zero;
        }

        // Find the closest waypoint to the current position
        for (int i = 0; i < wayPoints.Length; i++)
        {

            // get the distance between the current position and waypoint @ index
            float curDistance = Vector3.Distance(wayPoints[i], transform.position);


            // if this is the first time, the current distance IS the closest distance
            if (i == 0)
            {
                closestDistance = curDistance;
            }


            // if the current waypoint is closer to the current position
            // update the closest distance
            if (curDistance < closestDistance)
            {
                closestDistance = curDistance;
                closestWaypoint = i;
            }
        }

        // return the result
        return wayPoints[closestWaypoint];
    }


    // Draws the patrol points crosshairs
    void OnDrawGizmos()
    {
        if (globalWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < globalWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : globalWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }

}

