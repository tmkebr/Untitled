using UnityEngine;
using System.Collections;

public class Enemy_Sight : MonoBehaviour {

    public CharacterController2D player;
    public AIPath2D ai;

    void Start()
    {
        Debug.Log(ai);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // only pick up if the player is colliding with it
        if (!other.gameObject.CompareTag("Player")) return;

        //Debug.Log("colliding");
        //Debug.Log("Here " + other.transform.position);


        // the enemy is now alert
        ai.chase(player.transform);
        Debug.Log("Starting chase... ");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // only pick up if the player is colliding with it
        if (!other.gameObject.CompareTag("Player")) return;

        Debug.Log("leaving");

        // the enemy is no longer Alert
        // enemy.curStatus = Enemy_Patrol.Status.ALERT;
    }
}
