using UnityEngine;
using System.Collections;


[RequireComponent(typeof(LightDetector))]
[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(AIPath2D))]
public class Light_Damage : MonoBehaviour {

    LightDetector connectedDetector;
    AIPath2D enemyAI;
    Enemy enemy;
    public Light connectedLight;
    [Tooltip("The time it takes for the enemy to slow down. Arbitrary values. Acts like health.")]
    public float timeTillStop;
    [Tooltip("Damage multiplier. High  @ close range, low @ long range, avg in middle")]
    public float avgDamage, lowDamage, highDamage;
    [Tooltip("The start and end speed of the enemy as it takes damage.")]
    public float startSpeed, endSpeed;
    [Tooltip("% of damage calculated by distance from enemy to light.")]
    [Range(0, 1)]
    public float distanceWeight;
    [Tooltip("% of damage calculated by brightness of light on the enemy.")]
    [Range(0, 1)]
    public float brightnessWeight;
    [Tooltip("What is the current damage multiplier considering the distance and brightness?")]
    public float currentDamageMult;

    [Tooltip("The amount of knockback applied when damaged")]
    public float knockbackForce;
    [Tooltip("The frequency of the knockback applied when damage")]
    public float knockbackFrequency;
    
    float currentDecelTime, decelTime, t, weightedAvg, midpoint;
    float tParam = 0;
    float effectiveBrightness;
    float knockbackTimer;

    public SpriteRenderer colorSprite;

	// Use this for initialization
	void OnValidate () {
        // make sure the brightness and distance weight's = 100
        brightnessWeight = 1f - distanceWeight;

        connectedDetector = GetComponent<LightDetector>();
        enemyAI = GetComponent<AIPath2D>();
        enemy = GetComponent<Enemy>();

        // connect the starting speed to the connected enemy's speed
        startSpeed = enemyAI.speed;

        // set up the knockback timer
        knockbackTimer = knockbackFrequency;

        // get the colored sprite
        colorSprite = GetComponentInChildren<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {

        // if the Object is in light
        if (connectedDetector.inLight() == true)
        {
            Debug.Log("Lit!");

            // calculate the effective brightness and the weighted average
            effectiveBrightness = connectedDetector.effectiveBrightness(connectedDetector.distance);
            weightedAvg = (((brightnessWeight * effectiveBrightness) + (distanceWeight * (1/connectedDetector.distance))) / 2);
            timeTillStop = Mathf.Clamp(timeTillStop - weightedAvg, 0, timeTillStop);
            //print("weight = " + weightedAvg + "time = " + timeTillStop);

                // if it still needs to be lerped
                if (tParam < 1) {
                     tParam += Time.deltaTime / timeTillStop; // This will increment tParam based on Time.deltaTime multiplied by a speed multiplier
                     enemyAI.speed = Mathf.Lerp(startSpeed, endSpeed, tParam);
                }

                else if(!enemyAI.canMove)
                {
                // enemy is frozen
                Debug.Log("Enemy is frozen!");
                }


            // Calculate the appropriate damage
            // Damage the enemy
            // knock the enemy back
            currentDamageMult = calculateDamageAtDistance();
            enemy.DamageEnemy((int) currentDamageMult);
            knockback();
            desaturate(currentDamageMult);
        }

        // ELSE: not in the light
        // if the connected detector isn't in the light, there is no damage multiplier
        else
        {
            currentDamageMult = 0f;
        }
    }

    /** Calculates and returns a damage value based on distance
    * 
    */
    float calculateDamageAtDistance()
    {

        float distanceFactor, slope, distance, spriteWidth, intercept;
        
        spriteWidth = 0.6208f;
        distance = connectedDetector.distance;
        midpoint = (connectedLight.range) / 2;

        if (connectedDetector.distance >= midpoint)
        {
            // solve for damage at distance by calculating equation for the line
            // y = mx + b

            // m = delta(y)/delta(x)
            slope = ((lowDamage - avgDamage) / midpoint);

            //print(slope);

            // b = y - mx
            // will use y and x values for minDamage
            // y = minDamage, x = range
            intercept = lowDamage - (slope * (midpoint * 2));

            // y = mx + b

            distanceFactor = (slope * distance) + intercept;

            //distanceFactor = avgDamage - (slope * (distance - midpoint));
            // distanceFactor = distance * slope;

        }
        else
        {
            slope = ((avgDamage - highDamage) / (midpoint - spriteWidth));

            intercept = highDamage - slope * spriteWidth;

            distanceFactor = (slope * distance) + intercept;

            //distanceFactor = avgDamage + (slope * distance);
        }
        return distanceFactor;
    }

    void knockback()
    {
        // knock the enemy back if we are able to
        // else count down the frequency timer
        if (knockbackTimer <= 0)
        {
            // reset the timer
            knockbackTimer = knockbackFrequency;

            // get the normalized direction of the light
            Vector2 lightDir = connectedLight.transform.forward.normalized;

            // calculate the knockback vector as a product of the default force and the current damage multiplier
            //Vector2 knockback = (lightDir * knockbackForce * currentDamageMult);
            Vector2 knockback = (lightDir * knockbackForce);

            // knock the enemy back
            enemy.GetComponent<Rigidbody2D>().AddForce(knockback);
        }
        else
        {
            // decrease the cooldown timer
            knockbackTimer -= Time.deltaTime;
        }
    }

    void desaturate(float damage)
    {
        // amount to desaturate comes from a remapping of the damage to 0-1
        float desAmt = Remap(damage, lowDamage, highDamage, 0f, 1f);
        //Debug.Log("Remapped = " + desAmt);

        // get the old color and create the new color
        Color curColor = colorSprite.color;
        Color newColor = new Color(curColor.r, curColor.g, curColor.b, Mathf.Clamp(curColor.a - desAmt, 0f, 1f));
        Debug.Log(" R: " + newColor.r + " G: " + newColor.g + " B: " + newColor.b + " A: " + newColor.a);

        // update the opacity of the colored sprite
        colorSprite.color = newColor;
    }

    float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
