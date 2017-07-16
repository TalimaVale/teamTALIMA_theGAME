using UnityEngine;
using Photon;

/* Notes for Awesomeness class:
 * ****************************
 * Animation on prefab is not synced across network.
 *      Currently unnecessary because we use SphereCollider to collect coins.
 */

public class Awesomeness : PunBehaviour {

    int value = 1;

    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.name);
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null) {
            player.AddBawesomeness(value);
            photonView.RPC("Collect", PhotonTargets.MasterClient, photonView.viewID);
        }
    }

    [PunRPC]
    void Collect(int viewID) {
        PhotonNetwork.Destroy(PhotonView.Find(viewID).gameObject);
    }
}
