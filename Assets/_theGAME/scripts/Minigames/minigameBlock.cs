using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour {

    public minigameBlockStackConsole console;


    // heldItem acceptable dropDistance
    public float dropDistance = 5.0f;
    private float dropHover = 0.45f;

    private PlayerController owner;
    public bool hasOwner { get { return (owner != null); } }

    public bool stacked = false;

    void Awake() {
        gameObject.SetActive(false);
        console = FindObjectOfType<minigameBlockStackConsole>();
        console.Blocks.Add(gameObject);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer player) {
        // If we are the MasterClient
        if (photonView.isMine) {
            if (hasOwner) {
                photonView.RPC("SetBlockData", player, transform.position, transform.rotation, owner.photonView.viewID, stacked);
            } else {
                photonView.RPC("SetBlockData", player, transform.position, transform.rotation, 0, stacked);
            }
        }
    }

    public void Interact(int playerViewID) {
        GameObject interactingPlayer = PhotonView.Find(playerViewID).gameObject;

        if (!hasOwner) {
            if (interactingPlayer.GetComponent<PlayerController>().heldItem != null) return;
            photonView.RPC("PickUpBlock", PhotonTargets.All, playerViewID);

        } else if (hasOwner && owner.photonView.viewID == playerViewID) {
            photonView.RPC("DropBlock", PhotonTargets.All);

        } else Debug.Log("Attempt made to steal block");
    }



    /* RPC CALLS */

    [PunRPC] // called by MasterClient to OnPhotonPlayerConnected
    void SetBlockData(Vector3 pos, Quaternion rot, int ownerViewID, bool isStacked) {
        Debug.Log("<Color=Magenta>SetBlockData()</Color> -- Calling SetBlockData");

        // if block has owner
        if (ownerViewID != 0) {
            owner = PhotonView.Find(ownerViewID).GetComponent<PlayerController>();
            owner.heldItem = gameObject;
            gameObject.layer = LayerMask.NameToLayer("Player");

            transform.SetParent(owner.transform);
            transform.localPosition = owner.holdLocalVector;
            transform.localRotation = Quaternion.identity;
        // else if block is stacked
        } else if (isStacked) {
            transform.position = pos;
            transform.rotation = rot;

            GetComponent<BoxCollider>().enabled = false;
            gameObject.layer = 0;
            stacked = isStacked;
        // else block is free
        } else {
            transform.position = pos;
            transform.rotation = rot;
        }
        gameObject.SetActive(true);
    }

    [PunRPC] // called to All
    public void PickUpBlock(int playerViewID) {
        Debug.Log("<Color=Magenta>PickUpBlock()</Color> -- Calling PickUpBlock");

        owner = PhotonView.Find(playerViewID).GetComponent<PlayerController>();
        owner.heldItem = gameObject;
        gameObject.layer = LayerMask.NameToLayer("Player");

        transform.SetParent(owner.transform);
        transform.localPosition = owner.holdLocalVector;
        transform.localRotation = Quaternion.identity;
    }

    [PunRPC] // called to All
    void DropBlock() {
        Debug.Log("<Color=Magenta>DropBlock()</Color> -- Calling DropBlock");

        // Attempt to find 'ground'
        RaycastHit hit;
        if (Physics.BoxCast(owner.transform.position, new Vector3(.35f, .35f, .35f), Vector3.down, out hit, transform.rotation, dropDistance, -1)) {
            hit.point += new Vector3(0, (transform.localScale.y / 2) + dropHover, 0);
            transform.position = hit.point;
            
            owner.heldItem = null;
            owner = null;
            gameObject.layer = LayerMask.NameToLayer("Interact");
            transform.SetParent(null);

            // Console attempts to collect dropped blocks
            console.photonView.RPC("CollectBlocks", PhotonTargets.MasterClient);
        } else Debug.Log("No ground detected. Cannot drop block here.");
    }
}

// TODO: Improve (RPC)DropBlock Raycast, so blocks drop right in front of player (would require more complicated raycast)
// TODO: Take blocks off of camera script
// TODO: After block is held by player, that player sees the block as opaque (shader alpha channel moves from 200 to 255)