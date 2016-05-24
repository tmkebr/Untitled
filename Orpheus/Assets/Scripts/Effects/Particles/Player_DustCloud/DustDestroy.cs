using UnityEngine;
using System.Collections;

public class DustDestroy : MonoBehaviour {

    // will destroy itself when called
	void OnDestroy()
    {
        Destroy(gameObject);
    }
}
