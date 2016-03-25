using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    [System.Serializable]
    public class PlayerStats
    {
        public int maxHealth = 100;

        private int _curHealth;
        public int curHealth
        {
            get { return _curHealth; }
            set { _curHealth = Mathf.Clamp(value, 0, maxHealth); }
        }

        public void Init()
        {
            curHealth = maxHealth;
        }
    }

    public PlayerStats stats = new PlayerStats();

    // Knockback variables
    [Tooltip("How often is the player knocked back?")]
    public float knockbackFrequency = 0.5f;
    float knockbackTimer; // stores the knockback cooldown

    // sound variables
    //public AudioClip damageSound;
    //public AudioClip deathSound;
    private AudioSource theAudio;

    void Start()
    {
        stats.Init();

        knockbackTimer = knockbackFrequency;
    }

    public void DamagePlayer(int damage)
    {
        stats.curHealth -= damage;
        if (stats.curHealth <= 0)
        {
            GameManager.KillPlayer(this);
        }
    }

    public void knockback(Vector2 direction, float knockbackForce)
    {
        // knock the enemy back if we are able to
        // else count down the frequency timer
        //if (knockbackTimer <= 0)
        //{
            // reset the timer
            knockbackTimer = knockbackFrequency;

            // get the normalized direction of the light
            Vector2 lightDir = direction.normalized;

            // calculate the knockback vector as a product of the default force and the current damage multiplier
            //Vector2 knockback = (lightDir * knockbackForce * currentDamageMult);
            Vector2 knockback = (lightDir * knockbackForce);

            // knock the enemy back
            Debug.Log("Knocking player...");
        Debug.Log("rigidbody? " + GetComponent<Rigidbody2D>());
            GetComponent<Rigidbody2D>().AddForce(knockback);

            // play the damage sound
            //theAudio.PlayOneShot(damageSound);
        //}
        //else
        //{
        //    // decrease the cooldown timer
        //    Debug.Log("Cooling down " + knockbackTimer);
        //    knockbackTimer -= Time.deltaTime;
        //}
    }
}
