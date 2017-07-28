using UnityEngine;
using Photon;

/* Notes for Awesomeness class:
 * ****************************
 * Animation on prefab is not synced across network.
 *      Currently unnecessary because we use SphereCollider to collect coins.
 */

public class Awesomeness : PunBehaviour {

    public int value = 1;

    protected void OnTriggerEnter(Collider other) {
        Debug.Log(other.name);
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null && player.GetComponent<PhotonView>().isMine) {
            CallAddBawesomeness(player);
        }
    }

    virtual public void CallAddBawesomeness(PlayerController player) {
        player.AddBawesomeness(value);
        gameObject.SetActive(false);
        photonView.RPC("OnCollect", PhotonTargets.MasterClient, photonView.viewID);
    }

    [PunRPC]
    void OnCollect(int viewID) {
        Debug.Log("Awesomeness Collect RPC");
        PhotonNetwork.Destroy(PhotonView.Find(viewID).gameObject);
    }
}