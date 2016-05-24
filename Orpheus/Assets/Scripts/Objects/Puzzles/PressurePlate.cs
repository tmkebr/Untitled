using UnityEngine;
using System.Collections;

public class PressurePlate : MonoBehaviour {

    [Tooltip("The doors that open from this pressure plate")]
    //public ArrayList connectedDoors = new ArrayList();
    public Door connectedDoor;
    public string statueTag;

    
    // Use this for initialization
    void Start () {

        
	}
	
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("entered!");
        if (!other.gameObject.CompareTag(statueTag)) return;
        Debug.Log("enemy has entered!");
        connectedDoor.open();


        //foreach(Door door in connectedDoors)
        //{
            // open the doors
            //door.open();
        //}
    }
}
