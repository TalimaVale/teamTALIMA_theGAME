using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flying : MonoBehaviour {

    public int speed = 10;

	void Update () {
        transform.position = transform.position + new Vector3(0, 0, 1 * Time.deltaTime * speed);
        
        Debug.Log(transform.position);
	}
}
