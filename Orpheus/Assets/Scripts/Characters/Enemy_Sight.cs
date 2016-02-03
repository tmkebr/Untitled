using UnityEngine;
using System.Collections;

public class Enemy_Sight : MonoBehaviour {

    public Enemy_Patrol enemy;
    public CharacterController2D player;

    void OnTriggerEnter2D(Collider2D other)
    {
        // only pick up if the player is colliding with it
        if (!other.gameObject.CompareTag("Player")) return;

        Debug.Log("colliding");
        RaycastHit hit;
        Debug.Log("Here " + other.transform.position);
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            Debug.Log("Point of contact: " + hit.point);
        }

        // the enemy is now alert
        //enemy.curStatus = Enemy_Patrol.Status.ALERT;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // only pick up if the player is colliding with it
        if (!other.gameObject.CompareTag("Player")) return;

        Debug.Log("leaving");

        // the enemy is no longer Alert
        enemy.curStatus = Enemy_Patrol.Status.ALERT;
    }
}
