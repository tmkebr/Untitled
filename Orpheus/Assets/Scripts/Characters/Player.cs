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

    void Start()
    {
        stats.Init();
    }

    public void DamagePlayer(int damage)
    {
        stats.curHealth -= damage;
        if (stats.curHealth <= 0)
        {
            GameManager.KillPlayer(this);
        }
    } 
}
