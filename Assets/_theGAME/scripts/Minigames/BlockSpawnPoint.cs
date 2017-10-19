using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawnPoint : MonoBehaviour {

    public bool inUse = false;

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 1f);
    }
}