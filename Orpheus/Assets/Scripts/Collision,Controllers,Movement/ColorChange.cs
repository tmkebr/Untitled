using UnityEngine;
using System.Collections;

public class ColorChange : MonoBehaviour {

    //public Color color;

	// Use this for initialization
	void Start () {
        Color tmp = new Color(1, 1, 1, 0f);
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        //renderer.color = color;
        renderer.color = tmp;
	}
}
