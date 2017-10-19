using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCamera : MonoBehaviour {

    public Vector3 followDst;
    public Transform viewer;
	
    void Start() {
        transform.position = viewer.transform.position + followDst;
    }
    
	void Update () {
		transform.position = viewer.transform.position + followDst;
    }
}
