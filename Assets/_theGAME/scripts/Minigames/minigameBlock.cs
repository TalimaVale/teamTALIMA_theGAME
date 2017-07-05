using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour, IPunObservable {

    public minigameBlockStackConsole console;
    public float dropDistance = 5.0f;

    private PlayerController owner;
    public bool hasOwner { get { return (owner != null); } }

    void Awake() {
        gameObject.SetActive(false);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer player) {
        if (photonView.isMine) {
            if (hasOwner) {
                photonView.RPC("SetBlockData", player, transform.position, transform.rotation, owner.photonView.viewID);
            } else {
                photonView.RPC("SetBlockData", player, transform.position, transform.rotation, 0);
            }
        }
    }

    [PunRPC]
    void SetBlockData(Vector3 pos, Quaternion rot, int ownerViewID) {
        // if block has owner
        if (ownerViewID != 0) {
            owner = PhotonView.Find(ownerViewID).GetComponent<PlayerController>();
            owner.heldItem = gameObject;
            gameObject.layer = LayerMask.NameToLayer("Player");

            transform.SetParent(owner.transform, true);
            transform.localPosition = owner.holdLocalVector;
            transform.localRotation = Quaternion.identity;
        // else if no owner
        } else {
            transform.position = pos;
            transform.rotation = rot;
        }
        gameObject.SetActive(true);
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            //
        } else {
            //
        }
    }

    public void Interact(int playerViewID) {
        GameObject interactingPlayer = PhotonView.Find(playerViewID).gameObject;

        if (!hasOwner) {
            if (interactingPlayer.GetComponent<PlayerController>().heldItem != null) return;
            Debug.Log("Block is PICKED UP by player: " + interactingPlayer.GetPhotonView().owner.NickName);
            photonView.RPC("PickUpBlock", PhotonTargets.AllBuffered, playerViewID);

        } else if (hasOwner && owner.photonView.viewID == playerViewID) {
            Debug.Log("Block is DROPPED by player: " + interactingPlayer.GetPhotonView().owner.NickName);
            photonView.RPC("DropBlock", PhotonTargets.AllBuffered);

        } else {
            Debug.Log("Attempt made to steal block");
        }
    }

    [PunRPC]
    public void PickUpBlock(int playerViewID) {
        Debug.Log("<Color=Magenta>PickUpBlock()</Color> -- Calling PickUpBlock");

        owner = PhotonView.Find(playerViewID).GetComponent<PlayerController>();
        owner.heldItem = gameObject;
        gameObject.layer = LayerMask.NameToLayer("Player");

        transform.SetParent(owner.transform, true);
        transform.localPosition = owner.holdLocalVector;
        transform.localRotation = Quaternion.identity;
    }

    [PunRPC]
    void DropBlock() {
        // Attempt to find 'ground'
        RaycastHit hit;
        if (Physics.BoxCast(owner.transform.position, new Vector3(.35f, .35f, .35f), Vector3.down, out hit, transform.rotation, dropDistance, -1)) {
            hit.point += new Vector3(0, transform.localScale.y / 2, 0);
            transform.position = hit.point;
            
            owner.heldItem = null;
            owner = null;
            gameObject.layer = LayerMask.NameToLayer("Interact");
            transform.SetParent(null, true);
        } else {
            Debug.Log("No ground detected. Cannot drop block here.");
        }

        // Console attempts to collect blocks
        console.CollectBlocks();
    }
}

// TODO: Improve (RPC)DropBlock Raycast, so blocks drop right in front of player (would require more complicated raycast)
// TODO: Take blocks off of camera script