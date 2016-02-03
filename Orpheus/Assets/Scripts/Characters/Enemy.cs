using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    [System.Serializable]
    public class EnemyStats
    {
        public int maxHealth = 100;

        private int _curHealth;
        public int curHealth
        {
            get { return _curHealth; }
            set { _curHealth = Mathf.Clamp(value, 0, maxHealth); }
        }

        public int damage = 40;

        public void Init()
        {
            curHealth = maxHealth;
        }
    }

    public EnemyStats stats = new EnemyStats();

    public float curHealth;
    public bool isAlive;
    //public bool isGrounded;

	// Use this for initialization
	void Start () {

        // start up the enemy stats
        stats.Init();
	}

    public void DamageEnemy(int damage)
    {
        stats.curHealth -= damage;
        if (stats.curHealth <= 0)
        {
            GameManager.KillEnemy(this);
        }
    }

    public void killEnemy()
    {
        // the enemy is no longer alive, so it has 0 health
        isAlive = false;
        curHealth = 0f;
    }

    void OnCollisionEnter2D(Collision2D _colInfo)
    {
        Player _player = _colInfo.collider.GetComponent<Player>();
        if (_player != null)
        {
            _player.DamagePlayer(stats.damage);
            DamageEnemy(9999999);
        }
    }
}
