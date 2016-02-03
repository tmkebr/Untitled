using UnityEngine;
using System.Collections;


// maybe make the flashlight's transform a child of the parent
public class Flashlight : MonoBehaviour {

    // Public variables
    public Controller2D player; // the player containing information for position and velocity
    public KeyCode flashlightToggleKey = KeyCode.Mouse0; // key to turn the flashlight on/off (Mouse0 by default)
    public KeyCode flashlightUpKey = KeyCode.W;
    public KeyCode flashlightDownKey = KeyCode.S;
    public float defaultAngle, defaultRange, pivotSpeed; // the angle of the spotlight's cone in degrees, the range of the light, speed of movement
    public AudioClip clickSound;
    private AudioSource audio;


    // battery-related variables
    [Tooltip("The current battery level. Default 100")]
    public float charge = 100;
    [Tooltip("The maximum charged level of the flashlight. Default 100")]
    public float maxCharge = 100;
    [Tooltip("Speed at which the battery drains. Default 1")]
    public float drainSpeed = 1;

    [Tooltip("Maximum light intensity at a full charge. Default 2")]
    public float maxLightIntensity;
    [Tooltip("Minimum light intensity when drained. Default 0. Light Detection depends on this being 0")]
    public float minLightIntensity;

    [Tooltip("Unlimited charge for testing")]
    public bool testMode;

    // Offsets to align flashlight with player's hand
    Vector3 flashlightOffset;
    float yOff, xOff;
    float rotationOffset = 90;

    Light flashlight;  // the light the script is attached to

	// Use this for initialization
	void Start () {

        audio = GetComponent<AudioSource>();


        // set-up and store the light for use
        flashlight = GetComponent<Light>();
        flashlight.enabled = false;
        flashlight.spotAngle = defaultAngle;
        flashlight.range = defaultRange;

        // offset for flashlight positioning
        yOff = (float)-0.299;
        xOff = (float).22;
        flashlightOffset.y = yOff;
        flashlightOffset.x = xOff;

        // make the flashlight's (this) position the same as the player's
        transform.position = (player.transform.position + flashlightOffset); 
	}
	
	// Update is called once per frame
	void Update () {
        toggle(flashlightToggleKey); // check if the player has toggled the light off
        pivot(); // pivot the flashlight
        //transform.position = (player.transform.position + flashlightOffset); // make the flashlight's (this) position the same as the player's

        if (flashlight.enabled && !testMode)
        {
            // drain the battery
            charge = batteryDrain(charge);
            // dim the light relative to battery drain
            dimLight(charge);
        }
	}

    // TOGGLE
    // turns the flashlight on and off based on a buttonpress
    // @param: toggleKeyCode - the keycode that will toggle the flashlight. set by the public var
    void toggle(KeyCode toggleKeyCode){
        if (Input.GetKeyDown(toggleKeyCode) && (flashlight.enabled!= true)) {
            flashlight.enabled = true;
            audio.PlayOneShot(clickSound);
        }
        else if (Input.GetKeyDown(toggleKeyCode) && flashlight.enabled)
        {
            flashlight.enabled = false;
            audio.PlayOneShot(clickSound);
        }
    }

    // PIVOT
    // pivots the flashlight based on mouse position
    // param: none
    void pivot()
    {
        // subtracting the position of the player from the mouse position
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - flashlight.transform.position;
        difference.Normalize();		// normalizing the vector. Meaning that all the sum of the vector will be equal to 1

        float rotX = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;	// find the angle in degrees
        transform.rotation = Quaternion.Euler(-rotX, rotationOffset, rotationOffset);
    }

    // BATTERY DRAIN
    // drains the battery slowly over time from the max charge down to 0
    // param: the current charge level
    // returns: new charge level
    float batteryDrain(float curCharge)
    {
        float newCharge;
        return newCharge = Mathf.Clamp(curCharge - Time.deltaTime*drainSpeed, 0, maxCharge);
    }

    // DIM LIGHT
    // dims the light relative to battery level using a lerp function
    // param: the current charge level
    void dimLight(float charge)
    {
        flashlight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, charge / maxCharge);
    }

    public void updateCharge(float amount)
    {
        charge = Mathf.Clamp(charge + amount, 0, maxCharge);
    }


    void OnGUI() {
        GUI.Box(new Rect(10, 10, 100, 100), "charge = " + charge);       
    }

    // deprecated
    /*void pivot(KeyCode pivotUpKeyCode, KeyCode pivotDownKeyCode)
    {
        if (Input.GetKey(pivotUpKeyCode) && flashlight.enabled)
        {
            flashlight.transform.Rotate(Vector3.up * pivotSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(pivotDownKeyCode) && flashlight.enabled)
        {
            flashlight.transform.Rotate(Vector3.down * pivotSpeed * Time.deltaTime);
        }
    } */
}
