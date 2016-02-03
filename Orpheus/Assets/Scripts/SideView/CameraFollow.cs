using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	public Controller2D target; // the object to follow
	public float verticalOffset; // how far to move the camera vertically when target moves outside the focus area
	public float lookAheadDstX; // how far to move the camera horizontally when the target moves outside the focus area
	public float lookSmoothTimeX; // how long it takes for the look ahead to take place
	public float verticalSmoothTime; // how long it takes for the vertical offset to take place
	public Vector2 focusAreaSize; // the area to focus on. Larger areas require less camera updates
    public bool drawBounds; // controls the rendering of the square outline
    public bool camBounds; // whether to restrict camera movement or not
    public Vector3 maxCameraPos;
    public Vector3 minCameraPos;

    // the focus area the camera is following
	FocusArea focusArea;

	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirX;
	float smoothLookVelocityX;
	float smoothVelocityY;

	bool lookAheadStopped;

	void Start() {
		focusArea = new FocusArea (target.collider.bounds, focusAreaSize);
	}

    // done after the target's movement
	void LateUpdate() {
		focusArea.Update (target.collider.bounds); // update the focusarea's knowledge of the target's position 

		Vector2 focusPosition = focusArea.centre + Vector2.up * verticalOffset;

        // if the camera is moving (velocity isn't 0)
		if (focusArea.velocity.x != 0) {
            // update the lookahead
			lookAheadDirX = Mathf.Sign (focusArea.velocity.x);
			if (Mathf.Sign(target.playerInput.x) == Mathf.Sign(focusArea.velocity.x) && target.playerInput.x != 0) {
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirX * lookAheadDstX;
			}
			else {
				if (!lookAheadStopped) {
					lookAheadStopped = true;
					targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX)/4f; // update the X
				}
			}
		}

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

		


        // CAMERA BOUNDS
        // Confine the camera to a set of predefined boundaries
        if (camBounds)
        {

            focusPosition.y = Mathf.SmoothDamp(Mathf.Clamp(transform.position.y, minCameraPos.y, maxCameraPos.y), Mathf.Clamp(focusPosition.y, minCameraPos.y, maxCameraPos.y), ref smoothVelocityY, verticalSmoothTime);
            focusPosition.x = Mathf.Clamp((focusPosition.x + currentLookAheadX), minCameraPos.x, maxCameraPos.x);
        }
        // else disregard bounds when updating focus position
        else
        {

            focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
            focusPosition += Vector2.right * currentLookAheadX;
        }
        transform.position = (Vector3)focusPosition + Vector3.forward * -10;



	}

    // Draws a box to display the camera bounds
	void OnDrawGizmos() {
        if (drawBounds == true){
		Gizmos.color = new Color (1, 0, 0, .5f);
		Gizmos.DrawCube (focusArea.centre, focusAreaSize);
        }
	}

	struct FocusArea {
        //center of the focus area
		public Vector2 centre;
        // how far the focus area has moved since the last frame
		public Vector2 velocity;
		float left,right;
		float top,bottom;


        // create a focus area based on the size of the object being followed
        // @param: Bounds targetBounds - an object with a bounding box to follow
        // @param: Vector2 size - size of the follow focus area
		public FocusArea(Bounds targetBounds, Vector2 size) {

            // set up the the area based on target bound sizes
			left = targetBounds.center.x - size.x/2;
			right = targetBounds.center.x + size.x/2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			velocity = Vector2.zero;
			centre = new Vector2((left+right)/2,(top +bottom)/2);
		}

		public void Update(Bounds targetBounds) {
			float shiftX = 0;

            // if the target's bounding box is less than the left edge of our focus area, it's moving left...
			if (targetBounds.min.x < left) {
                // shift the focus area over left by shiftX amount
				shiftX = targetBounds.min.x - left;
			} 
            // else if the target's bounding box is greater than the right edge of our focus area, it's moving right...
            else if (targetBounds.max.x > right) {
                // shift the focus area over right by shiftX amount
				shiftX = targetBounds.max.x - right;
			}
            // do the shift
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom) {
				shiftY = targetBounds.min.y - bottom;
			} else if (targetBounds.max.y > top) {
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;
			centre = new Vector2((left+right)/2,(top +bottom)/2);
			velocity = new Vector2 (shiftX, shiftY);
		}
	}

}
