using UnityEngine;
using System.Collections;

// Attaches to an object and detects if the object's center is being hit by a spotlight
// Updates and stores boolean inLight

public class LightDetector : MonoBehaviour {

    public Light theLight; // the Light we're using
    bool facing; // will be used to store whether to light is facing the object (pointed at it)
    public bool lit; // is the object in the light?
    public float distance;
	
	// Update is called once per frame
	public bool inLight () {

        lit = false;
        // only look when the flashlight is enabled and charged
        if (theLight.enabled && theLight.intensity > 0)
        {
            // if the light has an angle
            if (theLight.spotAngle != null) {

                // Get the normalized direction towards the light and the distance between the object and the light
                Vector2 direction = transform.position - theLight.transform.position;
                distance = direction.magnitude; // get the distance
                //direction = direction / distance; // normalize the vector

                // if the object is facing the light (if the light is being shown onto the object)
                 if (isFacingLight())
                 {
                     // if the object is within the light's range
                     if (inRange(distance))
                     {
                         // was the object hit?
                         // whether the object is hit by the light or not. Currently only detects the center (origin = transform.position = center of the object)
                         lit = Physics2D.Raycast(transform.position, direction, theLight.range);
                     }
                 }
             }
        }

        //if (lit) print("LIT!");
        return lit;
	}

    public float effectiveBrightness(float d)
    {
        if (inRange(d)) {
            return theLight.intensity /  d;
        }
        else
        {
            return 0;
        }
    }

    // IS FACING LIGHT
    // determines if the light is being pointed at the object (facing it)
    // returns: boolean true if light is facing object
    bool isFacingLight()
    {
        // the angle for comparison is equal to half of the current spot angle (line is drawn thru center, distance to bottom of light cone is half)                 
        float angle = theLight.spotAngle / 2;
        // calculate whether the object is facing the light
        facing = Vector3.Angle(theLight.transform.forward, transform.position - theLight.transform.position) < angle;
        return facing;
    }

    // IN RANGE
    // determines if distance is within light's range
    // Takes a distance and compares it to the light's range
    // returns: boolean true if distance is within light's range
    bool inRange (float distance)
    {
        return distance < theLight.range;
    }


    public float getIntensity()
    {
        return theLight.intensity;
    }


}
