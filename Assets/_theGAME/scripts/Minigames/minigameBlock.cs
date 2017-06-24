using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour, IPunObservable {

    private Rigidbody rb;

    private PlayerController owner;
    private bool hasOwner { get { return (owner != null); } }

    private Vector3 holdLocalVector = new Vector3(0f, .5f, 1.1f);

    private Vector3 MostRecentNetworkPos; // public for the debug window, make private when done
    private Quaternion MostRecentNetworkRotation; // same as above
    public bool HasReceivedUpdates { get; private set; }
    private float lastNetworkUpdate;

    void Awake () {
        rb = GetComponent<Rigidbody>();

        MostRecentNetworkPos = new Vector3();
        MostRecentNetworkRotation = new Quaternion();
        HasReceivedUpdates = false;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        } else {
            HasReceivedUpdates = true;
            MostRecentNetworkPos = (Vector3)stream.ReceiveNext();
            MostRecentNetworkRotation = (Quaternion)stream.ReceiveNext();
            lastNetworkUpdate = Time.time;
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
        Debug.Log("New player connected " + newPlayer.NickName);
        //if (PhotonNetwork.isMasterClient && hasOwner) {
        //    photonView.RPC("ForceSync", newPlayer, owner.photonView.viewID, transform.position, transform.rotation);
        //}
        if (PhotonNetwork.isMasterClient) {
            if(hasOwner) photonView.RPC("ForceSync", newPlayer, transform.position, transform.rotation, owner.photonView.viewID);
            else photonView.RPC("ForceSync", newPlayer, transform.position, transform.rotation, 0);
        }
    }

    [PunRPC]
    void ForceSync(Vector3 serverPosition, Quaternion serverRotation, int parentViewID) {
        transform.position = serverPosition;
        transform.rotation = serverRotation;
        if(parentViewID != 0) PickUpBlock(parentViewID);
    }

    void Update() {
        /* 
		 * I want to think through this absolutely to make sure the flags are completely correct.
		 * If some changes are needed I'm not sure if this will hold. 
		 */

        if (!PhotonNetwork.isMasterClient && !hasOwner && HasReceivedUpdates) {
            if (transform.position != MostRecentNetworkPos) {
                //Vector3 newPos = Vector3.Lerp(transform.position, MostRecentNetworkPos, Time.deltaTime);

                Vector3 newPos = Vector3.Lerp(transform.position, MostRecentNetworkPos, (Time.time - lastNetworkUpdate) / (1f / (float)PhotonNetwork.sendRateOnSerialize));
                rb.MovePosition(newPos);
            }

            if (transform.rotation != MostRecentNetworkRotation) {
                Quaternion newRot = Quaternion.Lerp(transform.rotation, MostRecentNetworkRotation, (Time.time - lastNetworkUpdate) / (1f / (float)PhotonNetwork.sendRateOnSerialize));
                rb.MoveRotation(newRot);
            }
        }

        /*
		 * This is to keep updating the most recent transform even if we are the Master Client.
		 * 
		 * This is important because if we don't do this and the master client changes, the 
		 * MostRecentNetworkPos and MostRecentNetworkRotation will be zero.
		 */
        if (PhotonNetwork.isMasterClient) {
            MostRecentNetworkPos = transform.position;
            MostRecentNetworkRotation = transform.rotation;
        }


        //if (hasOwner) {
        //    // This code if we are not parenting to player
        //    //transform.rotation = owner.transform.rotation;
        //    //transform.position = owner.transform.position + (owner.transform.rotation * new Vector3(0f, .5f, 1.1f));
        //} else {
        //    //
        //}
    }

    public void Interact(int playerViewID) {
        GameObject InteractingPlayer = PhotonView.Find(playerViewID).gameObject;

        if (!hasOwner) {
            if (InteractingPlayer.GetComponent<PlayerController>().heldItem != null) return;
            
            //if (PhotonNetwork.player != photonView.owner) {
            //    Debug.Log("Request viewOwnership of Block");
            //    photonView.RequestOwnership();
            //}

            Debug.Log("Block is being picked up by player: " + InteractingPlayer.GetPhotonView().owner.NickName);
            photonView.RPC("PickUpBlock", PhotonTargets.AllBuffered, playerViewID);
        } else if(hasOwner && owner.photonView.viewID == playerViewID) {
            Debug.Log("Block is being DROPPED by player: " + InteractingPlayer.GetPhotonView().owner.NickName);
            photonView.RPC("DropBlock", PhotonTargets.AllBuffered);
        } else {
            Debug.Log("Attempt made to steal block");
        }
    }

    [PunRPC]
    void PickUpBlock(int playerViewID) {
        owner = PhotonView.Find(playerViewID).GetComponent<PlayerController>();
        owner.heldItem = gameObject;
        
        transform.SetParent(owner.transform, false);
        transform.localPosition = holdLocalVector;
        transform.localRotation = Quaternion.identity;
        rb.isKinematic = true;
    }

    [PunRPC]
    void DropBlock() {
        owner.heldItem = null;
        owner = null;

        transform.SetParent(null, true);
        rb.isKinematic = false;

        photonView.RPC("UpdateTransform", PhotonTargets.MasterClient, transform.position, transform.rotation);
    }

    [PunRPC]
    void UpdateTransform(Vector3 pos, Quaternion rot) {
        transform.position = pos;
        transform.rotation = rot;
    }
}