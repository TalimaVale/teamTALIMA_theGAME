using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour {

    private Transform console;
    private Rigidbody rb;

    public PlayerController owner;
    public bool hasOwner = false;

    // Use this for initialization
    void Start () {
        console = transform.parent;
        rb = GetComponent<Rigidbody>();
	}

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext(hasOwner);
        } else {
            hasOwner = (bool)stream.ReceiveNext();
        }
    }

    public void Interact(PlayerController player) {
        if(hasOwner == false) {
            if (player.heldItem != null) return;

            if (PhotonNetwork.player != photonView.owner) {
                Debug.Log("Request viewOwnership of Block");
                photonView.RequestOwnership();
            }

            Debug.Log("Block is being picked up by player");
            photonView.RPC("PickUpBlock", PhotonTargets.All, player.photonView.viewID);
        } else if(hasOwner == true && player == owner) {
            
            Debug.Log("Block is being DROPPED by player: " + player.name);
            photonView.RPC("DropBlock", PhotonTargets.All);
        } else {
            Debug.Log("Attempt made to steal block");
        }
    }

    [PunRPC]
    void PickUpBlock(int playerViewID) {
        hasOwner = true;
        owner = PhotonView.Find(playerViewID).GetComponent<PlayerController>();
        owner.heldItem = gameObject;
        
        rb.useGravity = false;
        rb.isKinematic = true;
        Transform t = owner.gameObject.transform;
        transform.SetParent(t);
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0.0f, 0.5f, 1.1f);
    }

    [PunRPC]
    void DropBlock() {
        hasOwner = false;
        owner.heldItem = null;
        owner = null;

        rb.useGravity = true;
        rb.isKinematic = false;
        transform.SetParent(console.transform);
    }
}
