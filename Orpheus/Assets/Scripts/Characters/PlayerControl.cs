using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class PlayerControl : MonoBehaviour {

    // 
    [Tooltip("Jump height with full press down")]
	public float maxJumpHeight = 4; // jump height with full press down
    [Tooltip("Jump height with minimal press down")]
	public float minJumpHeight = 1; // jump height with minimal press down
    [Tooltip("How long til reaching jump height. Determines jump speed")]
	public float timeToJumpApex = .4f; // time it takes to reach the height of the jump (jump speed)
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
    [Tooltip("How fast the object moves")]
	public float moveSpeed = 6; // speed of the object

    // Wall Jumping Directionals
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

    // Wall Jumping Speeds
	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;

    // Gravity/Speeds
	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;

	void Start() {

        // get the required components
		controller = GetComponent<Controller2D> (); // stores object's controls

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
		print ("Gravity: " + gravity + "  Jump Velocity: " + maxJumpVelocity);
	}

	void Update() {

        // stores the key press
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		int wallDirX = (controller.collisions.left) ? -1 : 1;

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);

		bool wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;

            // if current speed is greater (more negative than) max wall slide speed, slow down the player
			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}


			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (input.x != wallDirX && input.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				}
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
			else {
				timeToWallUnstick = wallStickTime;
			}

		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			if (wallSliding) {
				if (wallDirX == input.x) {
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				}
				else if (input.x == 0) {
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				}
				else {
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
				}
			}
			if (controller.collisions.below) {
				velocity.y = maxJumpVelocity;
			}
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}

	
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime, input);

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

	}
}
