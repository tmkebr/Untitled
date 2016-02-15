using UnityEngine;
using System.Collections;


[RequireComponent(typeof(LightDetector))]
[RequireComponent(typeof(Enemy))]
//[RequireComponent(typeof(EnemyAI))]
public class Light_Damage : MonoBehaviour {

    LightDetector connectedDetector;
    EnemyAI enemyAI;
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

    float currentDecelTime, decelTime, t, weightedAvg, midpoint;
    float tParam = 0;
    float effectiveBrightness;

	// Use this for initialization
	void OnValidate () {
        // make sure the brightness and distance weight's = 100
        brightnessWeight = 1f - distanceWeight;

        connectedDetector = GetComponent<LightDetector>();
        enemyAI = GetComponent<EnemyAI>();
        enemy = GetComponent<Enemy>();

        // connect the starting speed to the connected enemy's speed
        startSpeed = enemyAI.speed;
	}
	
	// Update is called once per frame
	void Update () {

        // if the Object is in light
        if (connectedDetector.inLight() == true)
        {

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

                else if(enemyAI.speed == 0)
                {
                // enemy is frozen
                Debug.Log("Enemy is frozen!");
                }


            // Calculate the appropriate damage
            // Damage the enemy
            currentDamageMult = calculateDamageAtDistance();
            enemy.DamageEnemy((int) currentDamageMult);
        }

        // ELSE: not in the light
        // if the connected detector isn't in the light, there is no damage multiplier
        else
        {
            currentDamageMult = 0f;
        }
    }

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


        //currentDecelTime += Time.deltaTime; 
}
