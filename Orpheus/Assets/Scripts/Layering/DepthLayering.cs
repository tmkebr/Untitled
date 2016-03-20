using UnityEngine;
using System.Collections;

public class DepthLayering : MonoBehaviour {

    //public CharacterController2D player;
    public Renderer player;
    float playerY;

    [Tooltip("The sorting layer value behind the player. Will be used when player is in front of object")]
    public string behindLayerName;
    [Tooltip("The sorting layer value of the player. Will be used when player is behind object")]
    public string frontLayerName;

    private SpriteRenderer theRenderer;

    void Start()
    {
        theRenderer = GetComponent<SpriteRenderer>();
    }

	// Update is called once per frame
	void Update () {
        playerY = player.transform.position.y;
        if (playerY > transform.position.y)
        {
            //Debug.Log("above");
            theRenderer.sortingLayerName = frontLayerName;
        }
        else
        {
            //Debug.Log("Below");
            theRenderer.sortingLayerName = behindLayerName;
        }
	}
}
