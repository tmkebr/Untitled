using UnityEngine;
using System.Collections;

public class DepthLayering : MonoBehaviour {

    //public CharacterController2D player;
    public Renderer player;
    float playerY;

    //[Tooltip ("The sorting layer value behind the player. Will be used when player is in front of object")]
    //public int behindLayerValue;
    //[Tooltip("The sorting layer value of the player. Will be used when player is behind object")]
    //public int frontLayerValue;

    public string behindLayerName;
    public string frontLayerName;

    private SpriteRenderer renderer;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();


        //behindLayerValue = player.sortingLayerID - 1;
        //frontLayerValue = player.sortingLayerID + 1;

        //Debug.Log(player.sortingLayerID);
    }

	// Update is called once per frame
	void Update () {
        playerY = player.transform.position.y;
        if (playerY > transform.position.y)
        {
            //Debug.Log("above");
            renderer.sortingLayerName = frontLayerName;
        }
        else
        {
            //Debug.Log("Below");
            renderer.sortingLayerName = behindLayerName;
        }
	}
}
