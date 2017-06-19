using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour, IPunObservable {

	private Rigidbody rb;
	private PlayerController Owner;

	public bool HasOwner
	{
		get
		{
			return (Owner != null);
		}
	}

	private Vector3 HoldLocalVector = new Vector3(0f, .5f, 1.1f);

	// Use this for initialization
	void Start ()
	{
        rb = GetComponent<Rigidbody>();
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		}
		else
		{
			Vector3 NewPos = (Vector3)stream.ReceiveNext();
			Quaternion NewRotation = (Quaternion)stream.ReceiveNext();

			if(!HasOwner)
			{
				transform.position = NewPos;
				transform.rotation = NewRotation;
			}
		}
	}

    void Update()
	{ 
		if(HasOwner && (transform.parent != Owner.transform))
		{
			Debug.Log("PANIC AHHHHH");
		}
    }

	public void Interact(int playerViewID)
	{
		GameObject InteractingPlayer = PhotonView.Find(playerViewID).gameObject;
		if(!HasOwner)
		{
            Debug.Log("Block is being picked up by player: " + InteractingPlayer.GetPhotonView().owner.NickName);
            photonView.RPC("PickUpBlock", PhotonTargets.AllBuffered, playerViewID);
        }
		else if(HasOwner && Owner.photonView.viewID == playerViewID)
		{
            Debug.Log("Block is being DROPPED by player: " + InteractingPlayer.GetPhotonView().owner.NickName);
            photonView.RPC("DropBlock", PhotonTargets.AllBuffered);
        }
		else
		{
            Debug.Log("Attempt made to steal block");
        }
    }

    [PunRPC]
    void PickUpBlock(int playerViewID)
	{
		Owner = PhotonView.Find(playerViewID).GetComponent<PlayerController>();

		transform.SetParent(Owner.transform, false);
		transform.localPosition = HoldLocalVector;
		transform.localRotation = Quaternion.identity;
		rb.isKinematic = true;
	}

    [PunRPC]
    void DropBlock()
	{
		Owner.heldItem = null;
		Owner = null;
		
		transform.SetParent(null, true);
		rb.isKinematic = false;
	}
}
