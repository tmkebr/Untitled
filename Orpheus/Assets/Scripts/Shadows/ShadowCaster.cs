using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class ShadowCaster : MonoBehaviour {

    public bool receiveShadow;

	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().receiveShadows = receiveShadow;
        GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
