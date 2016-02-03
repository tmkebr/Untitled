using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
    
    // Singleton game manager
    public static GameManager GM;

    void Awake() {
        if (GM == null) {
            GM = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        }
    }


    // Called from outside scripts
    public static void KillEnemy(Enemy enemy)
    {
        GM._KillEnemy(enemy);
    }

    // Kills the enemy and removes the gameObject from the scene
    public void _KillEnemy(Enemy _enemy)
    {
        //GameObject _clone = Instantiate(_enemy.deathParticles, _enemy.transform.position, Quaternion.identity) as GameObject;
        //Destroy(_clone, 5f);
        //cameraShake.Shake(_enemy.shakeAmt, _enemy.shakeLength);
        Destroy(_enemy.gameObject);
    }


    /// <summary>
    /// Kills the Player
    /// </summary>
    /// <param name="player"></param>
    public static void KillPlayer(Player player)
    {
        GM._KillPlayer(player);
    }

    public void _KillPlayer(Player _player)
    {
        Destroy(_player.gameObject);
    }
}
