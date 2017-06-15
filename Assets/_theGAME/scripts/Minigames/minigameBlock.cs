using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour {

    private Rigidbody rb;

    public PlayerController owner;
    public bool hasOwner = false;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
	}

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext(hasOwner);
        } else {
            hasOwner = (bool)stream.ReceiveNext();
        }
    }

    void Update() {
        if (owner != null) {
            rb.isKinematic = true;
            transform.rotation = owner.transform.rotation;
            transform.position = owner.transform.position + (owner.transform.rotation * new Vector3(0f, .5f, 1.1f));
        } else {
            rb.isKinematic = false;
            //transform.rotation = owner.transform.rotation;
            //owner.transform.TransformDirection(new Vector3(0f, .5f, 1.1f));
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
        
        //rb.useGravity = false;
        rb.isKinematic = true;
        //Transform t = owner.gameObject.transform;
        //transform.SetParent(t);
        //if (photonView.isMine && owner == PlayerController.localPlayer) {
        //    transform.localRotation = Quaternion.identity;
        //    transform.localPosition = new Vector3(0.0f, 0.5f, 1.1f);
        //}
    }

    [PunRPC]
    void DropBlock() {
        hasOwner = false;
        owner.heldItem = null;
        owner = null;

        //rb.useGravity = true;
        rb.isKinematic = false;
        //var p = transform.position;
        //transform.SetParent(console.transform);
        //transform.localPosition = console.transform.InverseTransformPoint(p);
    }
}
