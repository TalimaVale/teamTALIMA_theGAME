using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour {

    private Rigidbody rb;
    public minigameBlockStack console;

    public PlayerController owner;
    public bool hasOwner = false;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        console = transform.parent.GetComponent<minigameBlockStack>();
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
        }
        if(console.win && photonView.isMine) {
            // play destory animation/particle effect
            Debug.Log("Destroying block: " + console.win + ", " + photonView.isMine);
            PhotonNetwork.Destroy(photonView);
        }

        // if block falls, teleport it back
        if (owner == null && transform.position.y <= -5) transform.localPosition = new Vector3(Random.Range(-5, 5), console.blockSpawnHeight, Random.Range(-5, 5));
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
        
        rb.isKinematic = true;
    }

    [PunRPC]
    void DropBlock() {
        hasOwner = false;
        owner.heldItem = null;
        owner = null;
        
        rb.isKinematic = false;
    }
}
