using UnityEngine;
using System.Collections;

public class ShadowCast : MonoBehaviour {

    public bool receiveShadow;

    void Start()
    {
        GetComponent<Renderer>().receiveShadows = receiveShadow;
        GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }
}
