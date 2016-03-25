using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    [System.Serializable]
    public class EnemyStats
    {
        public int maxHealth = 100;
        public int damage = 40;

        private int _curHealth;
        public int curHealth
        {
            get { return _curHealth; }
            set { _curHealth = Mathf.Clamp(value, 0, maxHealth); }
        }

        public void Init()
        {
            Debug.Log("Setting health...");
            curHealth = maxHealth;
        }
    }

    public EnemyStats stats = new EnemyStats();

    // health variables
    public float curHealth;
    public bool isAlive;

    // Knockback variables
    [Tooltip("How often is the enemy knocked back?")]
    public float knockbackFrequency = 0.5f;
    [Tooltip("How strongly is the enemy knocked back?")]
    public float knockbackForce = 600f;
    [Tooltip("How strongly does the enemy knock the player back?")]
    public float playerknockbackForce = 400f;
    float knockbackTimer; // stores the knockback cooldown

    // sound variables
    public AudioClip damageSound;
    public AudioClip deathSound;
    private AudioSource theAudio;

    // Use this for initialization
    void Start () {

        // start up the enemy stats
        stats.Init();

        // set up the audiosource
        theAudio = GetComponent<AudioSource>();

        // set up the knockback timer
        knockbackTimer = knockbackFrequency;

        // by default, the enemy is alive
        isAlive = true;
	}

    public void DamageEnemy(int damage)
    {
        stats.curHealth -= damage;
        if (stats.curHealth <= 0)
        {
            killEnemy();
            GameManager.KillEnemy(this);
        }
    }

    public void killEnemy()
    {
        Debug.Log("enemy killed...");
        // the enemy is no longer alive, so it has 0 health
        isAlive = false;
        curHealth = 0f;

        // play the death sound
        theAudio.PlayOneShot(deathSound);
    }

    void OnCollisionEnter2D(Collision2D _colInfo)
    {
        Player _player = _colInfo.collider.GetComponent<Player>();

        // Damage the player if enemy is alive
        if (_player != null && isAlive)
        {
            Debug.Log("Damaging player!");
            _player.DamagePlayer(stats.damage);
            _player.knockback(transform.forward, 10000000f);
            //DamageEnemy(9999999);
        }
    }

    public void knockback(Vector2 direction)
    {
        // knock the enemy back if we are able to
        // else count down the frequency timer
        if (knockbackTimer <= 0)
        {
            // reset the timer
            knockbackTimer = knockbackFrequency;

            // get the normalized direction of the light
            Vector2 lightDir = direction.normalized;

            // calculate the knockback vector as a product of the default force and the current damage multiplier
            //Vector2 knockback = (lightDir * knockbackForce * currentDamageMult);
            Vector2 knockback = (lightDir * knockbackForce);

            // knock the enemy back
            GetComponent<Rigidbody2D>().AddForce(knockback);

            // play the damage sound
            theAudio.PlayOneShot(damageSound);
        }
        else
        {
            // decrease the cooldown timer
            knockbackTimer -= Time.deltaTime;
        }
    }
}
