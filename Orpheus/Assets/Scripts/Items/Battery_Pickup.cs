using UnityEngine;
using System.Collections;

public class Battery_Pickup : MonoBehaviour {

 public float powerChange = 10; //The amount of battery power to give, negative value hogs energy instead
 public Flashlight flashLight;
 
 void OnTriggerEnter2D(Collider2D other) {

     // only pick up if the player is colliding with it
     if (!other.gameObject.CompareTag("Player")) return; 
     // only pick up the power-up if the flashlight isn't full
     if (flashLight.charge < flashLight.maxCharge)
     {

        flashLight.updateCharge(powerChange);

        Destroy(gameObject);
     }
 }
}
