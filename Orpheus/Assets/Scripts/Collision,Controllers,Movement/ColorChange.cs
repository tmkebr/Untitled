using UnityEngine;
using System.Collections;

public class ColorChange : MonoBehaviour {

    public Color color;

	// Use this for initialization
	void Start () {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.color = color;
            //new Color(0.5f, 0.5f, 0.5f, 1f); // Set to opaque gray
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
